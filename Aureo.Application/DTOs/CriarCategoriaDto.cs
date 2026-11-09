using Aureo.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Aureo.Application.DTOs.Categoria
{
    public record CriarCategoriaDto(
        [property: Required, StringLength(50)] string Nome,
        [property: EnumDataType(typeof(TipoTransacao))] TipoTransacao Tipo,
        [property: RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Cor deve estar no formato #RRGGBB.")] string? Cor,
        [property: StringLength(50)] string? Icone
    );
}
