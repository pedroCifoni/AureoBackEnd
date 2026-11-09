using Aureo.Domain.Entities;

namespace Aureo.Domain.Interfaces
{
    public interface ICategoriaRepository
    {
        Task AddAsync(Categoria categoria, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<Categoria> categorias, CancellationToken ct = default);
        Task<IReadOnlyList<Categoria>> ListarAsync(Guid usuarioId, TipoTransacao? tipo, CancellationToken ct = default);
        Task<Categoria?> GetByIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default);
        Task<bool> ExisteNomeAsync(Guid usuarioId, string nome, TipoTransacao tipo, Guid? ignorarId, CancellationToken ct = default);
        Task<bool> EmUsoAsync(Guid id, CancellationToken ct = default);
        Task UpdateAsync(Categoria categoria, CancellationToken ct = default);
        Task DeleteAsync(Categoria categoria, CancellationToken ct = default);
    }
}
