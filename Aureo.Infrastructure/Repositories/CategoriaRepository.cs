using Aureo.Domain.Entities;
using Aureo.Domain.Interfaces;
using Aureo.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Aureo.Infrastructure.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoriaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Categoria categoria, CancellationToken ct = default)
        {
            await _context.Categorias.AddAsync(categoria, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task AddRangeAsync(IEnumerable<Categoria> categorias, CancellationToken ct = default)
        {
            await _context.Categorias.AddRangeAsync(categorias, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<Categoria>> ListarAsync(
            Guid usuarioId, TipoTransacao? tipo, CancellationToken ct = default)
        {
            var query = _context.Categorias
                .AsNoTracking()
                .Where(c => c.UsuarioId == usuarioId);

            if (tipo is { } t)
                query = query.Where(c => c.Tipo == t);

            return await query.OrderBy(c => c.Tipo).ThenBy(c => c.Nome).ToListAsync(ct);
        }

        // Rastreada de propósito: Transacao.Criar/Atualizar recebem esta instância como navegação.
        public async Task<Categoria?> GetByIdAsync(Guid id, Guid usuarioId, CancellationToken ct = default)
        {
            return await _context.Categorias
                .FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId, ct);
        }

        public async Task<bool> ExisteNomeAsync(
            Guid usuarioId, string nome, TipoTransacao tipo, Guid? ignorarId, CancellationToken ct = default)
        {
            var nomeNormalizado = nome.Trim().ToLower();

            return await _context.Categorias.AnyAsync(c =>
                c.UsuarioId == usuarioId
                && c.Tipo == tipo
                && c.Nome.ToLower() == nomeNormalizado
                && (ignorarId == null || c.Id != ignorarId), ct);
        }

        public async Task<bool> EmUsoAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Transacoes.AnyAsync(t => t.CategoriaId == id, ct);
        }

        public async Task UpdateAsync(Categoria categoria, CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Categoria categoria, CancellationToken ct = default)
        {
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync(ct);
        }
    }
}
