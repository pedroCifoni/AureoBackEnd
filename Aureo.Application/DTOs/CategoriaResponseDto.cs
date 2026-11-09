using Aureo.Domain.Entities;

namespace Aureo.Application.DTOs.Categoria
{
    public record CategoriaResponseDto(
        Guid Id,
        string Nome,
        TipoTransacao Tipo,
        string Cor,
        string? Icone
    );
}
