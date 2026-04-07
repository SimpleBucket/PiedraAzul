using Mediator;
using PiedraAzul.Domain.Entities.Operations;
using PiedraAzul.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace PiedraAzul.Application.Features.Appointments.CreateAppointment
{
    public class CreateAppointmentHandler
    : IRequestHandler<CreateAppointmentCommand, Appointment>
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IDoctorAvailabilitySlotRepository _slotRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IPatientGuestRepository _patientGuestRepository;

        public CreateAppointmentHandler(
            IAppointmentRepository appointmentRepository,
            IDoctorRepository doctorRepository,
            IDoctorAvailabilitySlotRepository slotRepository,
            IPatientRepository patientRepository,
            IPatientGuestRepository patientGuestRepository)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _slotRepository = slotRepository;
            _patientRepository = patientRepository;
            _patientGuestRepository = patientGuestRepository;
        }

        public async ValueTask<Appointment> Handle(
        CreateAppointmentCommand request,
        CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository
                .GetByIdAsync(request.DoctorId, cancellationToken);

            if (doctor is null)
                throw new Exception("Doctor not found");

            var slot = await _slotRepository
                .GetByIdAsync(request.SlotId, cancellationToken);

            if (slot is null)
                throw new Exception("Slot not found");

            if (request.PatientUserId is not null)
            {
                var patient = await _patientRepository
                    .GetByUserIdAsync(request.PatientUserId, cancellationToken);

                if (patient is null)
                    throw new Exception("Patient not found");
            }
            else if (request.PatientGuestId is not null)
            {
                var guest = await _patientGuestRepository
                    .GetByIdAsync(request.PatientGuestId, cancellationToken);

                if (guest is null)
                    throw new Exception("Guest not found");
            }
            else
            {
                throw new Exception("Patient required");
            }

            var exists = await _appointmentRepository
                .ExistsBySlotAndDateAsync(
                    request.SlotId,
                    request.Date,
                    cancellationToken);

            if (exists)
                throw new Exception("Slot already taken");

            var appointment = Appointment.Create(
                slot,
                request.Date,
                request.DoctorId,
                request.PatientUserId,
                request.PatientGuestId
            );

            await _appointmentRepository.AddAsync(appointment, cancellationToken);

            return appointment;
        }
    }
}
