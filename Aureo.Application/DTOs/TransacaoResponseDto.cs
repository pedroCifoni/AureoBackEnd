using Aureo.Domain.Entities;

namespace Aureo.Application.DTOs.Transacao
{
    public record TransacaoResponseDto(
        Guid Id,
        string Descricao,
        Guid CategoriaId,
        string Categoria,
        decimal Valor,
        TipoTransacao Tipo,
        DateTime Data
    );
}
