using HotChocolate;
using HotChocolate.Authorization;
using Mediator;
using Microsoft.AspNetCore.Http;
using PiedraAzul.Application.Features.Doctors.Queries.GetDoctorAppointments;
using PiedraAzul.Application.Features.Patients.Queries.GetPatientAppointments;
using PiedraAzul.GraphQL.Types;
using System.Security.Claims;

namespace PiedraAzul.GraphQL;

public partial class Query
{
    [Authorize]
    public async Task<List<AppointmentType>> GetDoctorAppointmentsAsync(
        string doctorId,
        DateTime? date,
        [Service] IMediator mediator,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new GraphQLException("No autenticado");

        var isAdmin = httpContextAccessor.HttpContext!.User.IsInRole("Admin");

        if (userId != doctorId && !isAdmin)
            throw new GraphQLException("No tienes permiso para ver estas citas");

        DateOnly? dateOnly = date.HasValue ? DateOnly.FromDateTime(date.Value) : null;
        var result = await mediator.Send(new GetDoctorAppointmentsQuery(doctorId, dateOnly));
        return result.Select(AppointmentType.FromDto).ToList();
    }

    [Authorize]
    public async Task<List<AppointmentType>> GetMyUpcomingAppointmentsAsync(
        [Service] IMediator mediator,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new GraphQLException("No autenticado");

        var result = await mediator.Send(new GetPatientAppointmentsQuery(userId, null));

        var now = DateTime.UtcNow.Date;
        return result
            .Where(a => a.Start >= now)
            .OrderBy(a => a.Start)
            .Select(AppointmentType.FromDto)
            .ToList();
    }

    [Authorize]
    public async Task<List<AppointmentType>> GetPatientAppointmentsAsync(
        string? patientUserId,
        string? patientGuestId,
        [Service] IMediator mediator,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new GraphQLException("No autenticado");

        var isAdmin = httpContextAccessor.HttpContext!.User.IsInRole("Admin");

        if (!string.IsNullOrEmpty(patientUserId) && userId != patientUserId && !isAdmin)
            throw new GraphQLException("No tienes permiso para ver estas citas");

        var result = await mediator.Send(new GetPatientAppointmentsQuery(patientUserId, patientGuestId));
        return result.Select(AppointmentType.FromDto).ToList();
    }
}
