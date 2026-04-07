
    namespace PiedraAzul.Application.Common.Interfaces
    {
        public interface IRefreshTokenService
        {
            // genera un token, lo guarda en la base de datos y lo retorna
            Task<string> GenerateRefreshTokenAsync(string userId);

            // retorna userId
            Task<string?> ValidateRefreshTokenAsync(string token);
            
            // recibe token viejo, retorna nuevo
            Task<string> RotateRefreshTokenAsync(string token);
        }
    }
