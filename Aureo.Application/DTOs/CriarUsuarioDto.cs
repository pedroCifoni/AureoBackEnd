using System.ComponentModel.DataAnnotations;

namespace Aureo.Application.DTOs
{
    public class CriarUsuarioDto
    {
        [Required, StringLength(100, MinimumLength = 3)]
        public string Nome { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(128, MinimumLength = 8)]
        public string Senha { get; set; } = string.Empty;
    }

    public class CriarUsuarioResponseDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; }
    }
}
