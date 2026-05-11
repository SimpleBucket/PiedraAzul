using HotChocolate;
using HotChocolate.Authorization;
using Mediator;
using Microsoft.AspNetCore.Http;
using PiedraAzul.Application.Common.Models.Patients;
using PiedraAzul.Application.Features.Appointments.CreateAppointment;
using PiedraAzul.GraphQL.Inputs;
using PiedraAzul.GraphQL.Types;
using System.Security.Claims;

namespace PiedraAzul.GraphQL;

public partial class Mutation
{
    [Authorize(Roles = new[] { "Doctor", "Admin" })]
    public async Task<AppointmentType> CreateAppointmentAsync(
        CreateAppointmentInput input,
        [Service] IMediator mediator,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        if (string.IsNullOrWhiteSpace(input.DoctorId))
            throw new GraphQLException("DoctorId requerido");

        var currentUserId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new GraphQLException("No autenticado");

        var isDoctor = httpContextAccessor.HttpContext!.User.IsInRole("Doctor");

        if (isDoctor && input.DoctorId != currentUserId)
            throw new GraphQLException("Los doctores solo pueden crear citas para sí mismos");

        if (!Guid.TryParse(input.DoctorAvailabilitySlotId, out var slotId))
            throw new GraphQLException("SlotId inválido");

        var date = DateOnly.FromDateTime(input.Date);

        GuestPatientRequest? patientGuest = null;

        if (input.Guest is not null)
        {
            patientGuest = new GuestPatientRequest
            {
                Identification = input.Guest.Identification,
                Name = input.Guest.Name,
                Phone = input.Guest.Phone,
                ExtraInfo = input.Guest.ExtraInfo
            };
        }

        var appointment = await mediator.Send(
            new CreateAppointmentCommand(
                input.DoctorId,
                slotId,
                date,
                input.PatientUserId,
                patientGuest
            )
        );

        return AppointmentType.FromDomain(appointment);
    }

    /// <summary>
    /// Booking público para invitados desde InstantBooking. No requiere autenticación.
    /// Solo acepta citas con datos de paciente invitado (Guest).
    /// </summary>
    public async Task<AppointmentType> BookGuestAppointmentAsync(
        CreateAppointmentInput input,
        [Service] IMediator mediator)
    {
        if (string.IsNullOrWhiteSpace(input.DoctorId))
            throw new GraphQLException("DoctorId requerido");

        if (input.Guest is null)
            throw new GraphQLException("Los datos del paciente son requeridos para el agendamiento como invitado");

        if (!Guid.TryParse(input.DoctorAvailabilitySlotId, out var slotId))
            throw new GraphQLException("SlotId inválido");

        var date = DateOnly.FromDateTime(input.Date);

        var patientGuest = new GuestPatientRequest
        {
            Identification = input.Guest.Identification,
            Name = input.Guest.Name,
            Phone = input.Guest.Phone,
            ExtraInfo = input.Guest.ExtraInfo
        };

        var appointment = await mediator.Send(
            new CreateAppointmentCommand(
                input.DoctorId,
                slotId,
                date,
                null,
                patientGuest
            )
        );

        return AppointmentType.FromDomain(appointment);
    }
}
