using Mediator;
using PiedraAzul.Application.Common.Interfaces;
using PiedraAzul.Domain.Entities.Profiles.Patients;
using PiedraAzul.Domain.Repositories;

namespace PiedraAzul.Application.Features.Patients.Commands.CreateGuestPatient;

public class CreateGuestPatientHandler
    : IRequestHandler<CreateGuestPatientCommand, string>
{
    private readonly IPatientGuestRepository _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditClient _audit;

    public CreateGuestPatientHandler(
        IPatientGuestRepository repo,
        IUnitOfWork unitOfWork,
        IAuditClient audit)
    {
        _repo       = repo;
        _unitOfWork = unitOfWork;
        _audit      = audit;
    }

    public async ValueTask<string> Handle(
        CreateGuestPatientCommand request,
        CancellationToken ct)
    {
        var id = await _unitOfWork.ExecuteAsync(async ct =>
        {
            var patient = new GuestPatient(
                Guid.NewGuid().ToString(),
                request.Name,
                request.Phone,
                request.ExtraInfo);

            await _repo.AddAsync(patient, ct);
            return patient.Id;
        }, ct);

        await _audit.LogAsync("Create", "GuestPatient", id, null, request.Name);
        return id;
    }
}