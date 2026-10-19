using Aureo.Application.DTOs.Transacao;
using Aureo.Domain.Common;

namespace Aureo.Application.Interfaces
{
    public interface ITransacaoService
    {
        Task<TransacaoResponseDto> CriarAsync(Guid usuarioId, CriarTransacaoDto dto);
        Task<PagedResult<TransacaoResponseDto>> ListarAsync(Guid usuarioId, ListarTransacoesQuery query);
        Task<TransacaoResponseDto> ObterPorIdAsync(Guid id, Guid usuarioId);
        Task<TransacaoResponseDto> AtualizarAsync(Guid id, Guid usuarioId, AtualizarTransacaoDto dto);
        Task DeletarAsync(Guid id, Guid usuarioId);
    }
}
