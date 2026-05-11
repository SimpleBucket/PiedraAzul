using PiedraAzul.Client.Models.Booking;
using PiedraAzul.Client.Models.GraphQL;
using PiedraAzul.Client.Models.UserProfiles;
using PiedraAzul.Client.Services.GraphQLServices;
using PiedraAzul.Client.Services.Utils;
using PiedraAzul.Client.States;
using PiedraAzul.Client.UI.Shared.Components.StepTag;
using Microsoft.AspNetCore.Components;

namespace PiedraAzul.Client.UI.Features.Booking.Pages
{
    public partial class InstantBooking
    {
        [Inject] private PatientSearchService PatientSearchService { get; set; } = default!;

        BookingModel Model = new();
        bool isLoading = false;
        bool isSuccess = false;

        Stepper<BookingModel> Stepper { get; set; }

        string? _errorMessage;

        // ── Patient Search / Verification Modal ───────────────────────────
        private bool _showVerificationModal;
        private bool _searching;
        private PatientSearchResultGQL? _searchResult;
        private string? _searchResultType;
        private string? _searchError;

        // ── OTP ──────────────────────────────────────────────────────────
        bool _otpSent = false;
        bool _otpLoading = false;
        string? _otpError = null;
        // ─────────────────────────────────────────────────────────────────

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            if (UserState.User != null)
                Navigation.NavigateTo("/medical-booking", forceLoad: false, replace: true);

            var response = await AuthService.GetCurrentUserAsync();
            if (response.IsSuccess)
                Navigation.NavigateTo("/medical-booking", forceLoad: false, replace: true);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender && Stepper != null)
            {
                Stepper.OnNext = async () =>
                {
                    if (Stepper.CurrentStep == 0)
                    {
                        // Step 1 → search DB before allowing advance
                        _searching = true;
                        _showVerificationModal = true;
                        _searchError = null;
                        _searchResult = null;
                        _searchResultType = null;
                        StateHasChanged();

                        await Task.Yield(); // let UI render the loading modal first

                        await SearchPatientByIdentificationAsync(Model.PatientIdentification!);

                        _searching = false;
                        StateHasChanged();

                        return false; // never auto-advance; modal handles navigation
                    }

                    return true;
                };
            }
        }

        // ── Patient Search ────────────────────────────────────────────────

        private async Task SearchPatientByIdentificationAsync(string identification)
        {
            // Usa el endpoint público sin auth (busca GuestPatient por cédula exacta)
            var result = await PatientSearchService.LookupByIdentificationAsync(identification);

            if (!result.IsSuccess)
            {
                _searchError = result.Error?.Message ?? "Error de conexión";
                _searchResultType = "ERROR";
                _searchResult = null;
                return;
            }

            if (result.Value is not null)
            {
                _searchResult = result.Value;
                _searchResultType = _searchResult.Type; // "GUEST"

                // Pre-populate model from search
                Model.PatientName = _searchResult.Name;
                Model.PatientPhone = _searchResult.Phone;
                Model.PatientDataFromSearch = true;
                Model.IsNewPatient = false;
            }
            else
            {
                _searchResultType = "NOT_FOUND";
                _searchResult = null;
                Model.PatientName = null;
                Model.PatientPhone = null;
                Model.PatientDataFromSearch = false;
                Model.IsNewPatient = true;
            }
        }

        // ── Modal Callbacks ───────────────────────────────────────────────

        private void OnRegisteredLogin()
        {
            _showVerificationModal = false;
            Navigation.NavigateTo($"/login?returnUrl=/instant-medical-booking");
        }

        private void OnRegisteredGuestContinue()
        {
            // Data already populated from search; skip data form (Step 2) → go to Doctor (Step 2 = index 2)
            _showVerificationModal = false;
            Model.IsNewPatient = false;
            Stepper.GoToStep(2);
            StateHasChanged();
        }

        private void OnGuestConfirm()
        {
            // Guest found, data pre-populated; skip data form → go to Doctor (index 2)
            _showVerificationModal = false;
            Model.IsNewPatient = false;
            Stepper.GoToStep(2);
            StateHasChanged();
        }

        private void OnNewPatientContinue()
        {
            // Not found or error fallback → show data form in Step 2 (index 1)
            _showVerificationModal = false;
            Model.IsNewPatient = true;
            Stepper.GoToStep(1);
            StateHasChanged();
        }

        private async Task OnRetrySearch()
        {
            _searching = true;
            _searchError = null;
            _searchResultType = null;
            StateHasChanged();

            await SearchPatientByIdentificationAsync(Model.PatientIdentification!);

            _searching = false;
            StateHasChanged();
        }

        private void OnVerificationModalClose()
        {
            _showVerificationModal = false;
        }

        // ── Doctor & Slot ─────────────────────────────────────────────────

        private void SelectedDoctor(DoctorModel? args)
        {
            if (args == null) return;
            Model.DoctorId = args.Id;
            Model.Doctor = args;
        }

        private void SelectSlot(AppointmentSchedulerModel args)
        {
            if (args == null) return;

            args.Time = args.Time.Replace("a. m.", "AM")
                                 .Replace("p. m.", "PM")
                                 .Replace("a.m.", "AM")
                                 .Replace("p.m.", "PM")
                                 .Trim();

            Model.SlotId = args.SlotId;
            Model.DayOfYear = args.Date;
            Model.AppointmentSchedulerModel = args;
        }

        // ── OTP ───────────────────────────────────────────────────────────

        private async Task SendOtpAsync()
        {
            _otpError = null;
            _otpLoading = true;
            StateHasChanged();

            var result = await AppointmentService.SendGuestOtpAsync(
                Model.PatientPhone!,
                Model.OtpChannel == "email" ? Model.PatientEmail : null,
                Model.OtpChannel);

            _otpLoading = false;

            if (!result.IsSuccess)
                _otpError = result.Error?.Message ?? "No se pudo enviar el código. Intenta de nuevo.";
            else
            {
                Model.OtpSessionToken = result.Value;
                _otpSent = true;
            }

            StateHasChanged();
        }

        private async Task VerifyOtpAsync()
        {
            if (string.IsNullOrEmpty(Model.OtpCode) || Model.OtpCode.Length != 6) return;

            _otpError = null;
            _otpLoading = true;
            StateHasChanged();

            var result = await AppointmentService.VerifyGuestOtpAsync(
                Model.OtpSessionToken!, Model.OtpCode);

            _otpLoading = false;

            if (!result.IsSuccess)
                _otpError = result.Error?.Message ?? "Error al verificar el código.";
            else if (!result.Value)
                _otpError = "Código incorrecto. Intenta de nuevo.";
            else
                Model.OtpVerified = true;

            StateHasChanged();
        }

        private void ResetOtp()
        {
            _otpSent = false;
            _otpError = null;
            Model.OtpSessionToken = null;
            Model.OtpCode = null;
            Model.OtpVerified = false;
        }

        // ── Submit ────────────────────────────────────────────────────────

        private async Task HandlerSubmit()
        {
            if (!Model.OtpVerified)
            {
                _errorMessage = "Debes verificar tu identidad antes de confirmar la cita.";
                return;
            }

            var extraInfo = Model.OtpChannel == "email" ? Model.PatientEmail ?? "" : "";

            var result = await AppointmentService.BookGuestAppointmentAsync(new CreateAppointmentGqlInput(
                Guest: CreateContracts.CreateGuestPatientInput(
                    Model.PatientName!, Model.PatientPhone!, Model.PatientIdentification!, extraInfo),
                PatientUserId: null,
                DoctorId: Model.DoctorId,
                DoctorAvailabilitySlotId: Model.SlotId,
                Date: Model.DayOfYear.ToUniversalTime()
            ));

            if (!result.IsSuccess)
            {
                _errorMessage = "Ocurrió un error al crear la cita. Por favor, inténtelo de nuevo.";
                return;
            }

            isLoading = false;
            isSuccess = true;
            Stepper?.GoToStep(0);
        }
    }
}
