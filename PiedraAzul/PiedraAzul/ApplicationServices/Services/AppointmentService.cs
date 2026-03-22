using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using PiedraAzul.Data;
using PiedraAzul.Data.Models;

namespace PiedraAzul.ApplicationServices.Services
{
    public interface IAppointmentService
    {
        Task<Appointment> CreateAppointmentAsync(Appointment appointment, string? patientUserId);

        Task<List<(DoctorAvailabilitySlot Slot, bool IsAvailable)>> GetDoctorDaySlotsAsync(Guid doctorId, DateTime date);
    }
    public class AppointmentService(IDbContextFactory<AppDbContext> dbContextFactory) : IAppointmentService
    {
        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment, string? patientUserId)
        {
            using var context = await dbContextFactory.CreateDbContextAsync();
            if(!string.IsNullOrWhiteSpace(patientUserId))
            {
                var patient = await context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == patientUserId);
                if (patient == null) throw new ArgumentNullException(nameof(patientUserId));
             
                appointment.PatientId = patient.PatientId;
            }
            context.Add(appointment);
            await context.SaveChangesAsync();

            return appointment;
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

    }
}
