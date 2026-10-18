using Aureo.Domain.Entities;

namespace Aureo.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task AddAsync(Usuario usuario);
        Task UpdateAsync(Usuario usuario);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario?> ObterPorNomeAsync(string nome);
    }
}
