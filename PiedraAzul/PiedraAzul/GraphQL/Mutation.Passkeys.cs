using HotChocolate;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Http;
using PiedraAzul.Application.Common.Interfaces;
using PiedraAzul.GraphQL.Inputs;
using PiedraAzul.GraphQL.Types;
using PiedraAzul.Services;
using System.Security.Claims;

namespace PiedraAzul.GraphQL;

public partial class Mutation
{
    [Authorize]
    public async Task<string> BeginPasskeyRegistrationAsync(
        BeginPasskeyRegistrationInput input,
        [Service] IPasskeyService passkeys,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new GraphQLException("No autenticado");

        var isAdmin = httpContextAccessor.HttpContext!.User.IsInRole("Admin");

        if (userId != input.UserId && !isAdmin)
            throw new GraphQLException("No tienes permiso para registrar una passkey en otra cuenta");

        return await passkeys.BeginRegistrationAsync(input.UserId, input.Email, input.DisplayName);
    }

    [Authorize]
    public async Task<bool> CompletePasskeyRegistrationAsync(
        CompletePasskeyRegistrationInput input,
        [Service] IPasskeyService passkeys,
        [Service] ILogger<Mutation> logger,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new GraphQLException("No autenticado");

        var isAdmin = httpContextAccessor.HttpContext!.User.IsInRole("Admin");

        if (userId != input.UserId && !isAdmin)
            throw new GraphQLException("No tienes permiso para completar una passkey en otra cuenta");

        try
        {
            var result = await passkeys.CompleteRegistrationAsync(
                input.UserId, input.AttestationResponse, input.FriendlyName);

            logger.LogInformation("Passkey registered successfully for user: {UserId} (Name: {FriendlyName})",
                input.UserId, input.FriendlyName);

            return result;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Passkey registration failed for user: {UserId} - {Error}",
                input.UserId, ex.Message);
            throw new GraphQLException(ex.Message);
        }
    }

    public async Task<string> BeginPasskeyAssertionAsync(
        [Service] IPasskeyService passkeys)
    {
        return await passkeys.BeginAssertionAsync();
    }

    public async Task<LoginResultType> CompletePasskeyAssertionAsync(
        CompletePasskeyAssertionInput input,
        [Service] IPasskeyService passkeys,
        [Service] Microsoft.AspNetCore.Identity.UserManager<PiedraAzul.Infrastructure.Identity.ApplicationUser> userManager,
        [Service] ILoginTokenService loginTokenService)
    {
        try
        {
            var (userId, roles) = await passkeys.CompleteAssertionAsync(input.AssertionResponse);

            var user = await userManager.FindByIdAsync(userId)
                ?? throw new GraphQLException("Usuario no encontrado");

            var loginToken = loginTokenService.CreateToken(user.Id);

            return new LoginResultType
            {
                User = new UserType
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email ?? "",
                    AvatarUrl = user.AvatarUrl,
                    Roles = roles,
                    EmailConfirmed = user.EmailConfirmed
                },
                LoginToken = loginToken
            };
        }
        catch (InvalidOperationException ex)
        {
            throw new GraphQLException(ex.Message);
        }
    }

    [Authorize]
    public async Task<bool> DeletePasskeyAsync(
        string passkeyId,
        [Service] IPasskeyService passkeys,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] ILogger<Mutation> logger)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new GraphQLException("No autenticado");

        if (!Guid.TryParse(passkeyId, out var id))
        {
            logger.LogWarning("Invalid passkey ID format attempted for user: {UserId}", userId);
            throw new GraphQLException("ID de passkey inválido");
        }

        var result = await passkeys.DeletePasskeyAsync(userId, id);

        if (result)
            logger.LogInformation("Passkey deleted for user: {UserId} (PasskeyId: {PasskeyId})", userId, passkeyId);

        return result;
    }
}
