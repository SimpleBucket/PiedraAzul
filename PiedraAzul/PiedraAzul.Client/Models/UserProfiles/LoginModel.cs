using System.ComponentModel.DataAnnotations;
namespace PiedraAzul.Client.Models.UserProfiles
{

    public class LoginModel
    {
        [Required(ErrorMessage = "Este campo es obligatorio")]
        public string? Login { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
        public string? Password { get; set; }
    }
}
