using HotChocolate;
using HotChocolate.Authorization;
using Mediator;
using Microsoft.AspNetCore.Http;
using PiedraAzul.Application.Common.Interfaces;
using PiedraAzul.GraphQL.Types;
using System.Security.Claims;

namespace PiedraAzul.GraphQL;

public partial class Query
{
    [Authorize]
    public async Task<bool> IsEmailOTPEnabledAsync(
        [Service] IMFAService mfaService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new GraphQLException("No autenticado");

        var status = await mfaService.GetMFAStatusAsync(userId);
        return status.EmailOTPEnabled;
    }

    [Authorize]
    public async Task<bool> IsTOTPEnabledAsync(
        [Service] IMFAService mfaService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new GraphQLException("No autenticado");

        var status = await mfaService.GetMFAStatusAsync(userId);
        return status.TOTPEnabled;
    }

    [Authorize]
    public async Task<bool> HasBackupCodesAsync(
        [Service] IMFAService mfaService,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new GraphQLException("No autenticado");

        var status = await mfaService.GetMFAStatusAsync(userId);
        return status.HasBackupCodes;
    }

    [Authorize]
    public async Task<bool> IsEmailConfirmedAsync(
        [Service] IMediator mediator,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new GraphQLException("No autenticado");

        var user = await mediator.Send(new PiedraAzul.Application.Features.Users.Queries.GetUserById.GetUserByIdQuery(userId));
        return user?.EmailConfirmed ?? false;
    }
}
