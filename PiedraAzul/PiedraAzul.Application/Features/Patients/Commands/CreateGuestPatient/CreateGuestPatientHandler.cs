using Mediator;
using PiedraAzul.Domain.Entities.Profiles.Patients;
using PiedraAzul.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace PiedraAzul.Application.Features.Patients.Commands.CreateGuestPatient
{

    public class CreateGuestPatientHandler
        : IRequestHandler<CreateGuestPatientCommand, string>
    {
        private readonly IPatientGuestRepository _repo;

        public CreateGuestPatientHandler(IPatientGuestRepository repo)
        {
            _repo = repo;
        }

        public async ValueTask<string> Handle(
            CreateGuestPatientCommand request,
            CancellationToken ct)
        {
            var patient = new GuestPatient(
                Guid.NewGuid().ToString(),
                request.Name,
                request.Phone,
                request.ExtraInfo
            );

            await _repo.AddAsync(patient, ct);

            return patient.Id;
        }
    }
}
