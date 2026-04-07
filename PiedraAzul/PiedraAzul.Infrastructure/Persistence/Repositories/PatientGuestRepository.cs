using Microsoft.EntityFrameworkCore;
using PiedraAzul.Domain.Entities.Profiles.Patients;
using PiedraAzul.Domain.Repositories;
using PiedraAzul.Infrastructure.Persistence;

namespace PiedraAzul.Infrastructure.Repositories;

public class PatientGuestRepository(IDbContextFactory<AppDbContext> factory) : IPatientGuestRepository
{
    public async Task<GuestPatient?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        using var context = await factory.CreateDbContextAsync(ct);

        return await context.Patients
            .OfType<GuestPatient>()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<GuestPatient>> SearchAsync(string text, CancellationToken ct = default)
    {
        using var context = await factory.CreateDbContextAsync(ct);

        return await context.Patients
            .OfType<GuestPatient>()
            .Where(x =>
                x.Name.Contains(text) ||
                x.Phone.Contains(text))
            .OrderBy(x => x.Name)
            .Take(10)
            .ToListAsync(ct);
    }

    public async Task AddAsync(GuestPatient patient, CancellationToken ct = default)
    {
        using var context = await factory.CreateDbContextAsync(ct);

        await context.Patients.AddAsync(patient, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(GuestPatient patient, CancellationToken ct = default)
    {
        using var context = await factory.CreateDbContextAsync(ct);

        context.Patients.Update(patient);
        await context.SaveChangesAsync(ct);
    }

    public Task<List<GuestPatient>> GetByIdsAsync(List<string> ids, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}