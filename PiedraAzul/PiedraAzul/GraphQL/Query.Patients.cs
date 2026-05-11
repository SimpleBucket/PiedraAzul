using HotChocolate;
using HotChocolate.Authorization;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PiedraAzul.Application.Features.Patients.Queries.SearchPatients;
using PiedraAzul.GraphQL.Types;
using PiedraAzul.Domain.Repositories;
using PiedraAzul.Infrastructure.Identity;

namespace PiedraAzul.GraphQL;

public partial class Query
{
    /// <summary>
    /// Lookup público (sin auth) para el flujo de auto-agendamiento.
    /// Busca primero en usuarios registrados y luego en pacientes invitados.
    /// Devuelve datos parcialmente enmascarados para cumplir con habeas data.
    /// </summary>
    public async Task<PatientSearchResultType?> LookupGuestByIdentificationAsync(
        string identification,
        [Service] IPatientGuestRepository guestRepository,
        [Service] UserManager<ApplicationUser> userManager)
    {
        if (string.IsNullOrWhiteSpace(identification))
            return null;

        var id = identification.Trim();

        // 1. Buscar en usuarios registrados
        var registeredUser = await userManager.Users
            .FirstOrDefaultAsync(u => u.IdentificationNumber == id && !u.IsDeleted);

        if (registeredUser is not null)
        {
            return new PatientSearchResultType
            {
                Id = registeredUser.Id,
                Name = MaskName(registeredUser.Name),
                Identification = id,
                Phone = MaskPhone(registeredUser.PhoneNumber ?? ""),
                Type = PatientTypeEnum.Registered
            };
        }

        // 2. Buscar en pacientes invitados
        var guest = await guestRepository.GetByIdAsync(id);
        if (guest is null) return null;

        return new PatientSearchResultType
        {
            Id = guest.Id,
            Name = MaskName(guest.Name),
            Identification = guest.Id,
            Phone = MaskPhone(guest.Phone ?? ""),
            Type = PatientTypeEnum.Guest
        };
    }

    [Authorize(Roles = new[] { "Doctor", "Admin" })]
    public async Task<List<PatientSearchResultType>> SearchPatientsAsync(
        string query,
        int? limit,
        [Service] IMediator mediator,
        [Service] UserManager<ApplicationUser> userManager)
    {
        var patients = await mediator.Send(new SearchPatientsQuery(query));
        var deduplicated = patients
            .GroupBy(p => p.Id)
            .Select(g => g.First())
            .OrderBy(p => p.Name)
            .Take(limit ?? int.MaxValue)
            .ToList();

        var results = new List<PatientSearchResultType>();
        foreach (var p in deduplicated)
        {
            var phone = p.Phone;
            if (p.Type == "Registered" && string.IsNullOrEmpty(phone))
            {
                var user = await userManager.FindByIdAsync(p.Id);
                phone = user?.PhoneNumber ?? "";
            }

            results.Add(new PatientSearchResultType
            {
                Id = p.Id,
                Name = p.Name,
                Identification = p.Type == "Guest" ? p.Id : "",
                Phone = phone,
                Type = p.Type == "Guest" ? PatientTypeEnum.Guest : PatientTypeEnum.Registered
            });
        }
        return results;
    }

    [Authorize(Roles = new[] { "Doctor", "Admin" })]
    public async Task<List<PatientSearchResultType>> SearchAutoCompletePatientsAsync(
        string query,
        [Service] IMediator mediator,
        [Service] UserManager<ApplicationUser> userManager)
    {
        var patients = await mediator.Send(new SearchPatientsQuery(query));

        var results = new List<PatientSearchResultType>();
        foreach (var p in patients)
        {
            var phone = p.Phone;
            if (p.Type == "Registered" && string.IsNullOrEmpty(phone))
            {
                var user = await userManager.FindByIdAsync(p.Id);
                phone = user?.PhoneNumber ?? "";
            }

            results.Add(new PatientSearchResultType
            {
                Id = p.Id,
                Name = p.Name,
                Identification = p.Type == "Guest" ? p.Id : "",
                Phone = phone,
                Type = p.Type == "Guest" ? PatientTypeEnum.Guest : PatientTypeEnum.Registered
            });
        }
        return results;
    }
    /// <summary>
    /// Enmascara el nombre: muestra los primeros 2 nombres, máximo 3 caracteres visibles
    /// por palabra, el resto como asteriscos según la longitud real de la palabra.
    /// </summary>
    /// <example>"Pepito Juarez Alcantarez" → "Pep**** Jua***"</example>
    private static string MaskName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "****";
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var masked = parts
            .Take(2)
            .Select(word =>
            {
                var visible = Math.Min(3, word.Length);
                return word[..visible] + new string('*', word.Length - visible);
            });
        return string.Join(" ", masked);
    }

    /// <summary>Enmascara el teléfono: muestra solo los últimos 4 dígitos.</summary>
    /// <example>"31234567890" → "****7890"</example>
    private static string MaskPhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length < 4) return "****";
        return "****" + digits[^4..];
    }
}
