using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using PiedraAzul.Data;
using PiedraAzul.Data.Models;

namespace PiedraAzul.ApplicationServices.Services
{
    public interface IAppointmentService
    {
        Task<Appointment> CreateAppointmentAsync(Appointment appointment, string? patientUserId = null, string? patientGuestId = null);

        Task<List<(DoctorAvailabilitySlot Slot, bool IsAvailable)>> GetDoctorDaySlotsAsync(Guid doctorId, DateTime date);

        Task<List<Appointment>> GetDoctorAppointmentsAsync(string doctorId, DateTime date = default);
        Task<List<Appointment>> GetPatientAppointmentsAsync(string? patientId, string? patientGuestId, DateTime date = default);

    }
    public class AppointmentService(IDbContextFactory<AppDbContext> dbContextFactory) : IAppointmentService
    {
        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment, string? patientUserId, string? patientGuestId = null)
        {
            using var context = await dbContextFactory.CreateDbContextAsync();
            if(!string.IsNullOrWhiteSpace(patientUserId))
            {
                var patient = await context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == patientUserId);
                if (patient == null) throw new ArgumentNullException(nameof(patientUserId));
             
                appointment.PatientId = patient.PatientId;
            }else if(!string.IsNullOrWhiteSpace(patientGuestId))
            {
                var patientGuest = await context.PatientGuests.FirstOrDefaultAsync(p => p.PatientIdentification == patientGuestId);
                if (patientGuest == null) throw new ArgumentNullException(nameof(patientGuestId));
                appointment.PatientGuestId = patientGuest.PatientIdentification;
            }
            else
            {
                throw new ArgumentException("Either patientUserId or patientGuestId must be provided");
            }
            context.Add(appointment);
            await context.SaveChangesAsync();

            return appointment;
        }

        public async Task<List<Appointment>> GetDoctorAppointmentsAsync(string doctorId, DateTime date = default)
        {
            using var context = await dbContextFactory.CreateDbContextAsync();

            var user = await context.DoctorProfiles
                .FirstOrDefaultAsync(doc => doc.UserId == doctorId);

            if (user == null)
                throw new ArgumentNullException(nameof(doctorId));

            var query = context.Appointments.AsQueryable();

            query = query.Where(a => a.DoctorId == user.DoctorId);

            if (date != default)
            {
                var day = date.Date;
                query = query.Where(a => a.Date == day);
            }

            return await query.ToListAsync();
        }

        public async Task<List<(DoctorAvailabilitySlot Slot, bool IsAvailable)>> GetDoctorDaySlotsAsync(Guid doctorId, DateTime date)
        {
            using var context = await dbContextFactory.CreateDbContextAsync();

            var dayOfWeek = date.DayOfWeek;
            var day = date.Date;

            var slots = await context.DoctorAvailabilitySlots
                .Where(s => s.DoctorId == doctorId && s.DayOfWeek == dayOfWeek)
                .ToListAsync();

            var occupied = await context.Appointments
                .Where(a => a.DoctorId == doctorId && a.Date == day)
                .Select(a => a.DoctorAvailabilitySlotId)
                .ToHashSetAsync();

            var result = slots
                .Select(slot => (
                    Slot: slot,
                    IsAvailable: !occupied.Contains(slot.Id)
                ))
                .ToList();

            return result;
        }

        public async Task<List<Appointment>> GetPatientAppointmentsAsync(
            string? patientId,
            string? patientGuestId,
            DateTime date = default)
        {
            using var context = await dbContextFactory.CreateDbContextAsync();

            var query = context.Appointments.AsQueryable();

            if (!string.IsNullOrEmpty(patientId))
            {
                var user = await context.PatientProfiles
                    .FirstOrDefaultAsync(p => p.UserId == patientId);

                if (user == null)
                    throw new ArgumentNullException(nameof(patientId));

                query = query.Where(a => a.PatientId == user.PatientId);
            }
            else if (!string.IsNullOrEmpty(patientGuestId))
            {
                query = query.Where(a => a.PatientGuestId == patientGuestId);
            }
            else
            {
                throw new ArgumentException("Debe proporcionar patientId o patientGuestId");
            }


            if (date != default)
            {
                var day = date.Date;
                query = query.Where(a => a.Date == day);
            }

            return await query
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.PatientGuest)
                .Include(a => a.DoctorAvailabilitySlot)
                .ToListAsync();
        }
    }
}
