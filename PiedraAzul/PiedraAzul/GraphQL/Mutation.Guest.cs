using HotChocolate;
using PiedraAzul.Application.Common.Interfaces;
using static PiedraAzul.Application.Common.Interfaces.OtpChannel;

namespace PiedraAzul.GraphQL;

public partial class Mutation
{
    /// <summary>
    /// Envía un OTP al huésped por WhatsApp o Email.
    /// Devuelve un sessionToken para verificar el código después.
    /// </summary>
    public async Task<string> SendGuestOtpAsync(
        string phone,
        string? email,
        string channel,
        [Service] IGuestOtpService guestOtp)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new GraphQLException("El teléfono es requerido.");

        var otpChannel = channel.ToLowerInvariant() switch
        {
            "whatsapp" => OtpChannel.WhatsApp,
            "email" => OtpChannel.Email,
            _ => throw new GraphQLException("Canal inválido. Usa 'whatsapp' o 'email'.")
        };

        if (otpChannel == OtpChannel.Email && string.IsNullOrWhiteSpace(email))
            throw new GraphQLException("El email es requerido para el canal Email.");

        try
        {
            return await guestOtp.SendAsync(phone, email, otpChannel);
        }
        catch (ArgumentException ex)
        {
            throw new GraphQLException(ex.Message);
        }
    }

    /// <summary>
    /// Verifica el código OTP del huésped.
    /// Retorna true si es válido, false si el código es incorrecto.
    /// Lanza excepción si expiró o se superaron los intentos.
    /// </summary>
    public async Task<bool> VerifyGuestOtpAsync(
        string sessionToken,
        string code,
        [Service] IGuestOtpService guestOtp)
    {
        if (string.IsNullOrWhiteSpace(sessionToken) || string.IsNullOrWhiteSpace(code))
            throw new GraphQLException("sessionToken y code son requeridos.");

        try
        {
            return await guestOtp.VerifyAsync(sessionToken, code);
        }
        catch (InvalidOperationException ex)
        {
            throw new GraphQLException(ex.Message);
        }
    }
}
