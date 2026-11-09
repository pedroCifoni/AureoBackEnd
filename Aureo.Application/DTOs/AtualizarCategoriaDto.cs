using System.ComponentModel.DataAnnotations;

namespace Aureo.Application.DTOs.Categoria
{
    // O tipo não muda: as transações existentes dependem dele.
    public record AtualizarCategoriaDto(
        [property: Required, StringLength(50)] string Nome,
        [property: RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Cor deve estar no formato #RRGGBB.")] string? Cor,
        [property: StringLength(50)] string? Icone
    );
}
