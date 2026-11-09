using Aureo.Domain.Entities;
using Aureo.Domain.Filtros;
using Aureo.Infrastructure.Context;
using Aureo.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Aureo.Tests.Repositories
{
    public class UsuarioRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UsuarioRepository _repository;

        public UsuarioRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new UsuarioRepository(_context);
        }

        public void Dispose() => _context.Dispose();

        [Fact]
        public async Task ListarAsync_FiltraPorNomeOuEmailEOrdenaPorNome()
        {
            await _repository.AddAsync(new Usuario("zeca", "zeca@x.com", "h"));
            await _repository.AddAsync(new Usuario("ana", "ana@maria.com", "h"));
            await _repository.AddAsync(new Usuario("maria", "m@x.com", "h"));

            var todos = await _repository.ListarAsync(null, 1, 20);
            var busca = await _repository.ListarAsync("MARIA", 1, 20);

            todos.Itens.Select(u => u.Nome).Should().ContainInOrder("ana", "maria", "zeca");
            busca.Itens.Select(u => u.Nome).Should().BeEquivalentTo("ana", "maria");
        }

        [Fact]
        public async Task ListarAsync_Pagina()
        {
            for (var i = 0; i < 5; i++)
                await _repository.AddAsync(new Usuario($"user{i}", $"u{i}@x.com", "h"));

            var pagina = await _repository.ListarAsync(null, 2, 2);

            pagina.Total.Should().Be(5);
            pagina.Itens.Should().HaveCount(2);
        }

        [Fact]
        public async Task ContarAdminsAsync_ContaSomenteAdmins()
        {
            var admin = new Usuario("admin", "a@x.com", "h");
            admin.AtualizarDados("admin", "a@x.com", PapelUsuario.Admin);
            await _repository.AddAsync(admin);
            await _repository.AddAsync(new Usuario("comum", "c@x.com", "h"));

            (await _repository.ContarAdminsAsync()).Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_RemoveUsuarioComTransacoes_SemAfetarOutros()
        {
            var alvo = new Usuario("alvo", "alvo@x.com", "h");
            var outro = new Usuario("outro", "outro@x.com", "h");
            await _repository.AddAsync(alvo);
            await _repository.AddAsync(outro);

            var catAlvo = Categoria.Criar("Lazer", TipoTransacao.Despesa, null, null, alvo.Id);
            var catOutro = Categoria.Criar("Lazer", TipoTransacao.Despesa, null, null, outro.Id);
            _context.Categorias.AddRange(catAlvo, catOutro);
            _context.Transacoes.AddRange(
                Transacao.Criar("a", catAlvo, 10m, TipoTransacao.Despesa, DateTime.UtcNow, alvo.Id),
                Transacao.Criar("b", catOutro, 20m, TipoTransacao.Despesa, DateTime.UtcNow, outro.Id));
            await _context.SaveChangesAsync();

            await _repository.DeleteAsync(alvo);

            (await _context.Usuarios.AnyAsync(u => u.Id == alvo.Id)).Should().BeFalse();
            (await _context.Transacoes.Where(t => t.UsuarioId == alvo.Id).CountAsync()).Should().Be(0);
            (await _context.Transacoes.Where(t => t.UsuarioId == outro.Id).CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task TransacaoRepository_ListarTodasAsync_TrazDonoEFiltraPorUsuario()
        {
            var a = new Usuario("ana", "ana@x.com", "h");
            var b = new Usuario("bia", "bia@x.com", "h");
            await _repository.AddAsync(a);
            await _repository.AddAsync(b);
            var catA = Categoria.Criar("Lazer", TipoTransacao.Despesa, null, null, a.Id);
            var catB = Categoria.Criar("Lazer", TipoTransacao.Despesa, null, null, b.Id);
            _context.Categorias.AddRange(catA, catB);
            _context.Transacoes.AddRange(
                Transacao.Criar("da ana", catA, 10m, TipoTransacao.Despesa, DateTime.UtcNow, a.Id),
                Transacao.Criar("da bia", catB, 20m, TipoTransacao.Despesa, DateTime.UtcNow, b.Id));
            await _context.SaveChangesAsync();
            var transacoes = new TransacaoRepository(_context);
            var filtro = new TransacaoFiltro(null, null, null, null, null, OrdenacaoTransacao.Data, true, 1, 20);

            var todas = await transacoes.ListarTodasAsync(null, filtro);
            var soDaBia = await transacoes.ListarTodasAsync(b.Id, filtro);

            todas.Total.Should().Be(2);
            todas.Itens.Select(t => t.Usuario.Nome).Should().BeEquivalentTo("ana", "bia");
            soDaBia.Itens.Should().ContainSingle().Which.Descricao.Should().Be("da bia");
        }
    }
}
