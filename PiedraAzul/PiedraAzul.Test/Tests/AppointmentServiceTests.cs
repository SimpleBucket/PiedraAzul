using Microsoft.EntityFrameworkCore;
using PiedraAzul.ApplicationServices.Services;
using PiedraAzul.Data;
using PiedraAzul.Data.Models;
using PiedraAzul.Shared.Enums;

namespace PiedraAzul.Test.Tests
{
    public class AppointmentServiceTests : IClassFixture<PostgresFixture>
    {
        private readonly PostgresFixture _fixture;
        private readonly AppointmentService _sut;

        public AppointmentServiceTests(PostgresFixture fixture)
        {
            _fixture = fixture;
            _sut = new AppointmentService(fixture.DbContextFactory);
        }

        // ─────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────

        private static DateTime UtcDate()
        {
            var now = DateTime.UtcNow;
            return new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        }

        private async Task<(DoctorProfile doctor, PatientProfile patient, ApplicationUser patientUser)> SeedUserPatientAsync()
        {
            await using var ctx = _fixture.DbContextFactory.CreateDbContext();

            var doctorUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "doc",
                Name = "Doctor Test"
            };

            var patientUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "pat",
                Name = "Patient Test"
            };

            var doctor = new DoctorProfile
            {
                DoctorId = Guid.NewGuid(),
                UserId = doctorUser.Id,
                Specialty = DoctorType.NaturalMedicine
            };

            var patient = new PatientProfile
            {
                PatientId = Guid.NewGuid(),
                UserId = patientUser.Id
            };

            ctx.Users.AddRange(doctorUser, patientUser);
            ctx.DoctorProfiles.Add(doctor);
            ctx.PatientProfiles.Add(patient);

            await ctx.SaveChangesAsync();

            return (doctor, patient, patientUser);
        }

        private async Task<(DoctorProfile doctor, PatientGuest guest)> SeedGuestAsync()
        {
            await using var ctx = _fixture.DbContextFactory.CreateDbContext();

            var doctorUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "doc-guest",
                Name = "Doctor Guest"
            };

            var doctor = new DoctorProfile
            {
                DoctorId = Guid.NewGuid(),
                UserId = doctorUser.Id,
                Specialty = DoctorType.NaturalMedicine
            };

            var guest = new PatientGuest
            {
                PatientIdentification = Random.Shared.Next(100000000, 999999999).ToString(),
                PatientName = "Guest Test",
                PatientPhone = "555-1234",
                PatientExtraInfo = "No extra info"
                
            };

            ctx.Users.Add(doctorUser);
            ctx.DoctorProfiles.Add(doctor);
            ctx.PatientGuests.Add(guest);

            await ctx.SaveChangesAsync();

            return (doctor, guest);
        }

        private async Task<DoctorAvailabilitySlot> SeedSlotAsync(Guid doctorId, DayOfWeek day)
        {
            await using var ctx = _fixture.DbContextFactory.CreateDbContext();

            var slot = new DoctorAvailabilitySlot
            {
                Id = Guid.NewGuid(),
                DoctorId = doctorId,
                DayOfWeek = day,
                StartTime = TimeOnly.Parse("09:00").ToTimeSpan(),
                EndTime = TimeOnly.Parse("09:30").ToTimeSpan()
            };

            ctx.DoctorAvailabilitySlots.Add(slot);
            await ctx.SaveChangesAsync();

            return slot;
        }

        // ─────────────────────────────────────────────
        // TESTS
        // ─────────────────────────────────────────────

        [Fact]
        public async Task CreateAppointment_WithPatientUser_Success()
        {
            var (doctor, patient, patientUser) = await SeedUserPatientAsync();
            var today = UtcDate();

            var slot = await SeedSlotAsync(doctor.DoctorId, today.DayOfWeek);

            var appointment = new Appointment
            {
                DoctorId = doctor.DoctorId,
                DoctorAvailabilitySlotId = slot.Id,
                Date = today
            };

            var result = await _sut.CreateAppointmentAsync(appointment, patientUserId: patientUser.Id);

            Assert.NotNull(result);
            Assert.Equal(patient.PatientId, result.PatientId);
        }

        [Fact]
        public async Task CreateAppointment_WithPatientGuest_Success()
        {
            var (doctor, guest) = await SeedGuestAsync();
            var today = UtcDate();

            var slot = await SeedSlotAsync(doctor.DoctorId, today.DayOfWeek);

            var appointment = new Appointment
            {
                DoctorId = doctor.DoctorId,
                DoctorAvailabilitySlotId = slot.Id,
                Date = today
            };

            var result = await _sut.CreateAppointmentAsync(appointment,string.Empty, patientGuestId: guest.PatientIdentification);

            Assert.NotNull(result);
            Assert.Equal(guest.PatientIdentification, result.PatientGuestId);
        }

        [Fact]
        public async Task CreateAppointment_NoIds_Throws()
        {
            var appointment = new Appointment { Date = UtcDate() };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CreateAppointmentAsync(appointment, null, null));
        }

        [Fact]
        public async Task CreateAppointment_InvalidUser_Throws()
        {
            var (doctor, _, _) = await SeedUserPatientAsync();
            var today = UtcDate();

            var slot = await SeedSlotAsync(doctor.DoctorId, today.DayOfWeek);

            var appointment = new Appointment
            {
                DoctorId = doctor.DoctorId,
                DoctorAvailabilitySlotId = slot.Id,
                Date = today
            };

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.CreateAppointmentAsync(appointment, patientUserId: Guid.NewGuid().ToString()));
        }

        [Fact]
        public async Task CreateAppointment_InvalidGuest_Throws()
        {
            var (doctor, _) = await SeedGuestAsync();
            var today = UtcDate();

            var slot = await SeedSlotAsync(doctor.DoctorId, today.DayOfWeek);

            var appointment = new Appointment
            {
                DoctorId = doctor.DoctorId,
                DoctorAvailabilitySlotId = slot.Id,
                Date = today
            };

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.CreateAppointmentAsync(appointment,string.Empty, patientGuestId: Guid.NewGuid().ToString()));
        }

        [Fact]
        public async Task GetDoctorDaySlots_ReturnsOccupiedAndAvailable()
        {
            var (doctor, patient, _) = await SeedUserPatientAsync();
            var today = UtcDate();

            var slot = await SeedSlotAsync(doctor.DoctorId, today.DayOfWeek);

            await using (var ctx = _fixture.DbContextFactory.CreateDbContext())
            {
                ctx.Appointments.Add(new Appointment
                {
                    Id = Guid.NewGuid(),
                    DoctorId = doctor.DoctorId,
                    PatientId = patient.PatientId,
                    DoctorAvailabilitySlotId = slot.Id,
                    Date = today,
                    CreatedAt = DateTime.UtcNow
                });

                await ctx.SaveChangesAsync();
            }

            var result = await _sut.GetDoctorDaySlotsAsync(doctor.DoctorId, today);

            Assert.Single(result);
            Assert.False(result[0].IsAvailable);
        }

        [Fact]
        public async Task GetDoctorAppointments_ReturnsAppointments()
        {
            var (doctor, patient, patientUser) = await SeedUserPatientAsync();
            var today = UtcDate();

            var slot = await SeedSlotAsync(doctor.DoctorId, today.DayOfWeek);

            await using (var ctx = _fixture.DbContextFactory.CreateDbContext())
            {
                ctx.Appointments.Add(new Appointment
                {
                    Id = Guid.NewGuid(),
                    DoctorId = doctor.DoctorId,
                    PatientId = patient.PatientId,
                    DoctorAvailabilitySlotId = slot.Id,
                    Date = today,
                    CreatedAt = DateTime.UtcNow
                });

                await ctx.SaveChangesAsync();
            }

            var result = await _sut.GetDoctorAppointmentsAsync(doctor.UserId);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetDoctorAppointments_InvalidDoctor_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.GetDoctorAppointmentsAsync(Guid.NewGuid().ToString()));
        }

        [Fact]
        public async Task GetPatientAppointments_WithUser_Returns()
        {
            var (doctor, patient, patientUser) = await SeedUserPatientAsync();
            var today = UtcDate();

            var slot = await SeedSlotAsync(doctor.DoctorId, today.DayOfWeek);

            await using (var ctx = _fixture.DbContextFactory.CreateDbContext())
            {
                ctx.Appointments.Add(new Appointment
                {
                    Id = Guid.NewGuid(),
                    DoctorId = doctor.DoctorId,
                    PatientId = patient.PatientId,
                    DoctorAvailabilitySlotId = slot.Id,
                    Date = today,
                    CreatedAt = DateTime.UtcNow
                });

                await ctx.SaveChangesAsync();
            }

            var result = await _sut.GetPatientAppointmentsAsync(patientUser.Id, null);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetPatientAppointments_WithGuest_Returns()
        {
            var (doctor, guest) = await SeedGuestAsync();
            var today = UtcDate();

            var slot = await SeedSlotAsync(doctor.DoctorId, today.DayOfWeek);

            await using (var ctx = _fixture.DbContextFactory.CreateDbContext())
            {
                ctx.Appointments.Add(new Appointment
                {
                    Id = Guid.NewGuid(),
                    DoctorId = doctor.DoctorId,
                    PatientGuestId = guest.PatientIdentification,
                    DoctorAvailabilitySlotId = slot.Id,
                    Date = today,
                    CreatedAt = DateTime.UtcNow
                });

                await ctx.SaveChangesAsync();
            }

            var result = await _sut.GetPatientAppointmentsAsync(null, guest.PatientIdentification);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetPatientAppointments_NoIds_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetPatientAppointmentsAsync(null, null));
        }

        [Fact]
        public async Task GetPatientAppointments_InvalidUser_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.GetPatientAppointmentsAsync(Guid.NewGuid().ToString(), null));
        }
    }
}