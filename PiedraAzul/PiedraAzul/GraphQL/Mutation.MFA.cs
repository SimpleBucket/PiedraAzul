using HotChocolate;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Http;
using PiedraAzul.Application.Common.Interfaces;
using PiedraAzul.GraphQL.Inputs;
using System.Security.Claims;

namespace PiedraAzul.GraphQL;

public partial class Mutation
{
    [Authorize]
    public async Task<List<string>> EnableMFAAsync(
        EnableMFAInput input,
        [Service] IMFAService mfaService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] ILogger<Mutation> logger)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new GraphQLException("No autenticado");

        var backupCodes = await mfaService.EnableMFAAsync(userId, input.Method);
        if (backupCodes.Count == 0)
        {
            logger.LogWarning("Failed to enable MFA for user: {UserId}", userId);
            throw new GraphQLException("No se pudo activar la autenticación de dos factores");
        }

        logger.LogInformation("MFA enabled for user: {UserId} with method: {Method}", userId, input.Method);
        return backupCodes;
    }

    [Authorize]
    public async Task<bool> DisableMFAAsync(
        DisableMFAInput input,
        [Service] IMFAService mfaService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] ILogger<Mutation> logger)
    {
        if (!input.Confirm)
            throw new GraphQLException("Debe confirmar para deshabilitar la autenticación de dos factores");

        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new GraphQLException("No autenticado");

        var result = await mfaService.DisableMFAAsync(userId, input.Method);
        if (!result)
        {
            logger.LogWarning("Failed to disable MFA for user: {UserId}", userId);
            throw new GraphQLException("No se pudo deshabilitar la autenticación de dos factores");
        }

        logger.LogInformation("MFA disabled for user: {UserId} with method: {Method}", userId, input.Method);
        return true;
    }

    [Authorize]
    public async Task<bool> InitiateMFAVerificationAsync(
        [Service] IMFAService mfaService,
        [Service] IEmailService emailService,
        [Service] Microsoft.AspNetCore.Identity.UserManager<PiedraAzul.Infrastructure.Identity.ApplicationUser> userManager,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] ILogger<Mutation> logger)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new GraphQLException("No autenticado");

        var isMFAEnabled = await mfaService.IsEnabledAsync(userId);
        if (!isMFAEnabled)
            throw new GraphQLException("La autenticación de dos factores no está habilitada");

        var otp = await mfaService.GenerateOTPAsync(userId);
        var user = await userManager.FindByIdAsync(userId);

        if (user?.Email is null)
        {
            logger.LogWarning("Email not found for user: {UserId}", userId);
            throw new GraphQLException("No se pudo enviar el código de verificación");
        }

        var emailSent = await emailService.SendMFAEmailAsync(user.Email, user.Name ?? user.Email, otp, 10);
        if (!emailSent)
        {
            logger.LogWarning("Failed to send MFA email to user: {UserId}", userId);
            throw new GraphQLException("No se pudo enviar el código de verificación");
        }

        logger.LogInformation("MFA verification initiated for user: {UserId}", userId);
        return true;
    }

    public async Task<bool> VerifyMFAAsync(
        VerifyMFAInput input,
        [Service] IMFAService mfaService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] ILogger<Mutation> logger)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            throw new GraphQLException("No autenticado");

        var isValid = await mfaService.VerifyOTPAsync(userId, input.OTP);
        if (!isValid)
        {
            logger.LogWarning("Invalid MFA verification attempt for user: {UserId}", userId);
            throw new GraphQLException("Código de verificación inválido");
        }

        logger.LogInformation("MFA verification successful for user: {UserId}", userId);
        return true;
    }

    [Authorize]
    public async Task<bool> VerifyBackupCodeAsync(
        VerifyMFAInput input,
        [Service] IMFAService mfaService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] ILogger<Mutation> logger)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            throw new GraphQLException("No autenticado");

        var isValid = await mfaService.VerifyBackupCodeAsync(userId, input.OTP);
        if (!isValid)
        {
            logger.LogWarning("Invalid backup code verification attempt for user: {UserId}", userId);
            throw new GraphQLException("Código de recuperación inválido");
        }

        logger.LogInformation("Backup code verified for user: {UserId}", userId);
        return true;
    }

    [Authorize]
    public async Task<bool> VerifyTOTPAsync(
        VerifyMFAInput input,
        [Service] IMFAService mfaService,
        [Service] IHttpContextAccessor httpContextAccessor,
        [Service] ILogger<Mutation> logger)
    {
        var userId = httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            throw new GraphQLException("No autenticado");

        var isValid = await mfaService.VerifyTOTPAsync(userId, input.OTP);
        if (!isValid)
        {
            logger.LogWarning("Invalid TOTP verification attempt for user: {UserId}", userId);
            throw new GraphQLException("Código TOTP inválido");
        }

        logger.LogInformation("TOTP verification successful for user: {UserId}", userId);
        return true;
    }
}
