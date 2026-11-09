using Aureo.Domain.Common;
using Aureo.Domain.Entities;
using Aureo.Domain.Filtros;
using Aureo.Domain.Interfaces;
using Aureo.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Aureo.Infrastructure.Repositories
{
    public class TransacaoRepository : ITransacaoRepository
    {
        private readonly ApplicationDbContext _context;

        public TransacaoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Transacao transacao)
        {
            await _context.Transacoes.AddAsync(transacao);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<Transacao>> ListarAsync(Guid usuarioId, TransacaoFiltro filtro)
        {
            var query = _context.Transacoes
                .AsNoTracking()
                .Include(t => t.Categoria)
                .Where(t => t.UsuarioId == usuarioId);

            return await PaginarAsync(query, filtro, CancellationToken.None);
        }

        public async Task<PagedResult<Transacao>> ListarTodasAsync(
            Guid? usuarioId, TransacaoFiltro filtro, CancellationToken ct = default)
        {
            var query = _context.Transacoes
                .AsNoTracking()
                .Include(t => t.Categoria)
                .Include(t => t.Usuario)
                .AsQueryable();

            if (usuarioId is { } id)
                query = query.Where(t => t.UsuarioId == id);

            return await PaginarAsync(query, filtro, ct);
        }

        private static async Task<PagedResult<Transacao>> PaginarAsync(
            IQueryable<Transacao> query, TransacaoFiltro filtro, CancellationToken ct)
        {
            if (filtro.DataInicio is { } inicio)
                query = query.Where(t => t.Data >= inicio);

            if (filtro.DataFim is { } fim)
                query = query.Where(t => t.Data <= fim);

            if (filtro.Tipo is { } tipo)
                query = query.Where(t => t.Tipo == tipo);

            if (filtro.CategoriaId is { } categoriaId)
                query = query.Where(t => t.CategoriaId == categoriaId);

            if (!string.IsNullOrWhiteSpace(filtro.Busca))
            {
                var busca = filtro.Busca.Trim().ToLower();
                query = query.Where(t => t.Descricao.ToLower().Contains(busca));
            }

            var total = await query.CountAsync(ct);

            // Id como desempate mantém a paginação estável.
            query = (filtro.OrdenarPor, filtro.Decrescente) switch
            {
                (OrdenacaoTransacao.Valor, true) => query.OrderByDescending(t => t.Valor).ThenBy(t => t.Id),
                (OrdenacaoTransacao.Valor, false) => query.OrderBy(t => t.Valor).ThenBy(t => t.Id),
                (_, true) => query.OrderByDescending(t => t.Data).ThenBy(t => t.Id),
                _ => query.OrderBy(t => t.Data).ThenBy(t => t.Id)
            };

            var itens = await query
                .Skip((filtro.Pagina - 1) * filtro.Tamanho)
                .Take(filtro.Tamanho)
                .ToListAsync(ct);

            return new PagedResult<Transacao>(itens, filtro.Pagina, filtro.Tamanho, total);
        }

        public async Task<Transacao?> GetByIdAsync(Guid id, Guid usuarioId)
        {
            return await _context.Transacoes
                .Include(t => t.Categoria)
                .FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == usuarioId);
        }

        // A entidade já vem rastreada de GetByIdAsync; Update() marcaria também a Categoria como modificada.
        public async Task UpdateAsync(Transacao transacao)
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Transacao transacao)
        {
            _context.Transacoes.Remove(transacao);
            await _context.SaveChangesAsync();
        }
    }
}
