namespace PiedraAzul.GraphQL;

/// <summary>
/// GraphQL Mutation root type. Dividida en partial classes por feature:
/// - Mutation.Auth.cs: Login, Register, Password Reset, MFA
/// - Mutation.Appointments.cs: Crear citas
/// - Mutation.Users.cs: Gestión de usuarios (Admin)
/// - Mutation.Passkeys.cs: Passkeys
/// - Mutation.MFA.cs: MFA en detalle
/// - Mutation.Account.cs: Perfil de usuario
/// - Mutation.Guest.cs: OTP para invitados
/// - Mutation.Schedule.cs: Configuración de horarios
/// </summary>
public partial class Mutation
{
}
