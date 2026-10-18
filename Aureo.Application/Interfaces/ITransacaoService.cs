using Aureo.Application.DTOs.Transacao;

namespace Aureo.Application.Interfaces
{
    public interface ITransacaoService
    {
        Task<TransacaoResponseDto> CriarAsync(Guid usuarioId, CriarTransacaoDto dto);
        Task<IEnumerable<TransacaoResponseDto>> ObterTodasAsync(Guid usuarioId);
        Task<TransacaoResponseDto> ObterPorIdAsync(Guid id, Guid usuarioId);
        Task DeletarAsync(Guid id, Guid usuarioId);
    }
}
