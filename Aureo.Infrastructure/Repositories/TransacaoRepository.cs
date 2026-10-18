using System;
using System.Collections.Generic;
using System.Text;

using Aureo.Domain.Entities;
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

        public async Task<IEnumerable<Transacao>> GetAllByUsuarioAsync(Guid usuarioId)
        {
            return await _context.Transacoes
                .AsNoTracking()
                .Where(t => t.UsuarioId == usuarioId)
                .OrderByDescending(t => t.Data)
                .ToListAsync();
        }

        public async Task<Transacao?> GetByIdAsync(Guid id, Guid usuarioId)
        {
            return await _context.Transacoes
                .FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == usuarioId);
        }

        public async Task DeleteAsync(Transacao transacao)
        {
            _context.Transacoes.Remove(transacao);
            await _context.SaveChangesAsync();
        }
    }
}