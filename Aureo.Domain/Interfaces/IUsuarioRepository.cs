using Aureo.Domain.Common;
using Aureo.Domain.Entities;

namespace Aureo.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task AddAsync(Usuario usuario);
        Task UpdateAsync(Usuario usuario);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario?> ObterPorNomeAsync(string nome);

        Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<PagedResult<Usuario>> ListarAsync(string? busca, int pagina, int tamanho, CancellationToken ct = default);
        Task<int> ContarAdminsAsync(CancellationToken ct = default);
        // Remove o usuário com suas transações e categorias.
        Task DeleteAsync(Usuario usuario, CancellationToken ct = default);
    }
}
