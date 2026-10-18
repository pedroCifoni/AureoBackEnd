using Aureo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aureo.Domain.Interfaces
{
    public interface ITransacaoRepository
    {
        Task AddAsync(Transacao transacao);
        Task<IEnumerable<Transacao>> GetAllByUsuarioAsync(Guid usuarioId);
        Task<Transacao?> GetByIdAsync(Guid id, Guid usuarioId);
        Task DeleteAsync(Transacao transacao);
    }
}
