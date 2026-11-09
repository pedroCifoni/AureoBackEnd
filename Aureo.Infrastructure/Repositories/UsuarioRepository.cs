using Aureo.Domain.Common;
using Aureo.Domain.Entities;
using Aureo.Domain.Interfaces;
using Aureo.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Aureo.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ApplicationDbContext _context;

        public UsuarioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Usuario?> ObterPorNomeAsync(string nome)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Nome == nome);
        }

        public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);
        }

        public async Task<PagedResult<Usuario>> ListarAsync(
            string? busca, int pagina, int tamanho, CancellationToken ct = default)
        {
            var query = _context.Usuarios.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                query = query.Where(u => u.Nome.ToLower().Contains(termo) || u.Email.ToLower().Contains(termo));
            }

            var total = await query.CountAsync(ct);

            var itens = await query
                .OrderBy(u => u.Nome).ThenBy(u => u.Id)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .ToListAsync(ct);

            return new PagedResult<Usuario>(itens, pagina, tamanho, total);
        }

        public async Task<int> ContarAdminsAsync(CancellationToken ct = default)
        {
            return await _context.Usuarios.CountAsync(u => u.Papel == PapelUsuario.Admin, ct);
        }

        // As transações saem na mesma gravação, antes do usuário: a FK Transacao->Categoria é RESTRICT
        // e o cascade do banco apagaria as categorias antes delas.
        public async Task DeleteAsync(Usuario usuario, CancellationToken ct = default)
        {
            var transacoes = await _context.Transacoes
                .Where(t => t.UsuarioId == usuario.Id)
                .ToListAsync(ct);

            _context.Transacoes.RemoveRange(transacoes);
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync(ct);
        }
    }
}
