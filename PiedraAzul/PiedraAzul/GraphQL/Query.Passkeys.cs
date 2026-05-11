using HotChocolate;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Http;
using PiedraAzul.Application.Common.Interfaces;
using PiedraAzul.GraphQL.Types;
using System.Security.Claims;

namespace PiedraAzul.GraphQL;

public partial class Query
{
    [Authorize]
    public async Task<List<PasskeyType>> GetMyPasskeysAsync(
        [Service] IPasskeyService passkeys,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new GraphQLException("No autenticado");

        var list = await passkeys.GetUserPasskeysAsync(userId);

        return list.Select(p => new PasskeyType
        {
            Id = p.Id.ToString(),
            FriendlyName = p.FriendlyName,
            CreatedAt = p.CreatedAt
        }).ToList();
    }
}
