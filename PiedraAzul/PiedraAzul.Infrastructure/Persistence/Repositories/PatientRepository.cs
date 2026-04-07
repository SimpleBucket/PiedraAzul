using Microsoft.EntityFrameworkCore;
using PiedraAzul.Domain.Entities.Profiles.Patients;
using PiedraAzul.Domain.Repositories;
using PiedraAzul.Infrastructure.Persistence;

namespace PiedraAzul.Infrastructure.Repositories;

public class PatientRepository(IDbContextFactory<AppDbContext> factory) : IPatientRepository
{
    public async Task<RegisteredPatient?> GetByUserIdAsync(string userId, CancellationToken ct = default)
    {
        using var context = await factory.CreateDbContextAsync(ct);

        return await context.Patients
            .OfType<RegisteredPatient>()
            .FirstOrDefaultAsync(x => x.UserId == userId, ct);
    }

    public async Task<IReadOnlyList<RegisteredPatient>> SearchAsync(string text, CancellationToken ct = default)
    {
        using var context = await factory.CreateDbContextAsync(ct);

        return await context.Patients
            .OfType<RegisteredPatient>()
            .Where(x => x.Name.Contains(text))
            .OrderBy(x => x.Name)
            .Take(10)
            .ToListAsync(ct);
    }

    public async Task AddAsync(RegisteredPatient patient, CancellationToken ct = default)
    {
        using var context = await factory.CreateDbContextAsync(ct);

        await context.Patients.AddAsync(patient, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RegisteredPatient patient, CancellationToken ct = default)
    {
        using var context = await factory.CreateDbContextAsync(ct);

        context.Patients.Update(patient);
        await context.SaveChangesAsync(ct);
    }
}