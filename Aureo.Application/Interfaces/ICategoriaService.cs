using Aureo.Application.DTOs.Categoria;
using Aureo.Domain.Entities;

namespace Aureo.Application.Interfaces
{
    public interface ICategoriaService
    {
        Task<IReadOnlyList<CategoriaResponseDto>> ListarAsync(Guid usuarioId, TipoTransacao? tipo, CancellationToken ct = default);
        Task<CategoriaResponseDto> ObterPorIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default);
        Task<CategoriaResponseDto> CriarAsync(Guid usuarioId, CriarCategoriaDto dto, CancellationToken ct = default);
        Task<CategoriaResponseDto> AtualizarAsync(Guid id, Guid usuarioId, AtualizarCategoriaDto dto, CancellationToken ct = default);
        Task DeletarAsync(Guid id, Guid usuarioId, CancellationToken ct = default);
    }
}
