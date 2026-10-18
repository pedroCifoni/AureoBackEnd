using System.ComponentModel.DataAnnotations;

namespace Aureo.Application.DTOs
{
    public class LoginDto
    {
        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string Senha { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
