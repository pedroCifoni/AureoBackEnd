using Aureo.Domain.Common;
using Aureo.Domain.Entities;
using Aureo.Domain.Filtros;

namespace Aureo.Domain.Interfaces
{
    public interface ITransacaoRepository
    {
        Task AddAsync(Transacao transacao);
        Task<PagedResult<Transacao>> ListarAsync(Guid usuarioId, TransacaoFiltro filtro);
        Task<Transacao?> GetByIdAsync(Guid id, Guid usuarioId);
        Task UpdateAsync(Transacao transacao);
        Task DeleteAsync(Transacao transacao);
    }
}
