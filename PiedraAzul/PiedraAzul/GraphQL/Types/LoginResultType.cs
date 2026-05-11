namespace PiedraAzul.GraphQL.Types;

public class LoginResultType
{
    public UserType? User { get; set; }
    public MFARequiredType? MFARequired { get; set; }
    /// <summary>Token de un solo uso (60 s) para aplicar la cookie en /auth/apply-session.</summary>
    public string? LoginToken { get; set; }
}
