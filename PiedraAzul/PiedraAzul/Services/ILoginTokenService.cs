namespace PiedraAzul.Services;

/// <summary>
/// Crea y consume tokens de sesión de un solo uso (TTL ~60 s) para que el
/// browser pueda aplicar la cookie de autenticación en un request HTTP directo,
/// evitando el problema de Set-Cookie cuando la petición GraphQL pasa por el
/// circuito Blazor Server en lugar de llegar directamente desde el navegador.
/// </summary>
public interface ILoginTokenService
{
    /// <summary>Genera un token aleatorio asociado a <paramref name="userId"/> y lo guarda en caché.</summary>
    string CreateToken(string userId);

    /// <summary>Valida y elimina el token. Devuelve el userId o null si el token no existe/expiró.</summary>
    string? ConsumeToken(string token);
}
