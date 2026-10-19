using Aureo.Domain.Entities;
using Aureo.Domain.Filtros;
using Aureo.Infrastructure.Context;
using Aureo.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Aureo.Tests.Repositories
{
    public class TransacaoRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly TransacaoRepository _repository;
        private readonly Guid _usuarioId = Guid.NewGuid();
        private readonly Guid _outroUsuarioId = Guid.NewGuid();

        public TransacaoRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new TransacaoRepository(_context);
        }

        public void Dispose() => _context.Dispose();

        private static DateTime Dia(int dia) => new(2026, 10, dia, 12, 0, 0, DateTimeKind.Utc);

        private static TransacaoFiltro Filtro(
            DateTime? inicio = null, DateTime? fim = null, TipoTransacao? tipo = null,
            string? categoria = null, string? busca = null,
            OrdenacaoTransacao ordenar = OrdenacaoTransacao.Data, bool desc = true,
            int pagina = 1, int tamanho = 20)
            => new(inicio, fim, tipo, categoria, busca, ordenar, desc, pagina, tamanho);

        private async Task SeedAsync()
        {
            _context.Transacoes.AddRange(
                Transacao.Criar("Salário", "Renda", 5000m, TipoTransacao.Receita, Dia(5), _usuarioId),
                Transacao.Criar("Supermercado", "Alimentação", 300m, TipoTransacao.Despesa, Dia(10), _usuarioId),
                Transacao.Criar("Padaria", "Alimentação", 20m, TipoTransacao.Despesa, Dia(15), _usuarioId),
                Transacao.Criar("Cinema", "Lazer", 60m, TipoTransacao.Despesa, Dia(20), _usuarioId),
                Transacao.Criar("Alheia", "Renda", 999m, TipoTransacao.Receita, Dia(10), _outroUsuarioId));
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task ListarAsync_NaoRetornaTransacoesDeOutroUsuario()
        {
            await SeedAsync();

            var resultado = await _repository.ListarAsync(_usuarioId, Filtro());

            resultado.Total.Should().Be(4);
            resultado.Itens.Should().NotContain(t => t.Descricao == "Alheia");
        }

        [Fact]
        public async Task ListarAsync_OrdenaPorDataDescendentePorPadrao()
        {
            await SeedAsync();

            var resultado = await _repository.ListarAsync(_usuarioId, Filtro());

            resultado.Itens.Select(t => t.Descricao)
                .Should().ContainInOrder("Cinema", "Padaria", "Supermercado", "Salário");
        }

        [Fact]
        public async Task ListarAsync_OrdenaPorValorAscendente()
        {
            await SeedAsync();

            var resultado = await _repository.ListarAsync(_usuarioId,
                Filtro(ordenar: OrdenacaoTransacao.Valor, desc: false));

            resultado.Itens.Select(t => t.Valor).Should().BeInAscendingOrder();
        }

        [Fact]
        public async Task ListarAsync_FiltraPorPeriodoInclusivo()
        {
            await SeedAsync();

            var resultado = await _repository.ListarAsync(_usuarioId, Filtro(inicio: Dia(10), fim: Dia(15)));

            resultado.Itens.Select(t => t.Descricao).Should().BeEquivalentTo("Supermercado", "Padaria");
        }

        [Fact]
        public async Task ListarAsync_FiltraPorTipo()
        {
            await SeedAsync();

            var resultado = await _repository.ListarAsync(_usuarioId, Filtro(tipo: TipoTransacao.Receita));

            resultado.Itens.Should().ContainSingle().Which.Descricao.Should().Be("Salário");
        }

        [Fact]
        public async Task ListarAsync_FiltraPorCategoriaSemDiferenciarCaixa()
        {
            await SeedAsync();

            var resultado = await _repository.ListarAsync(_usuarioId, Filtro(categoria: "ALIMENTAÇÃO"));

            resultado.Total.Should().Be(2);
        }

        [Fact]
        public async Task ListarAsync_BuscaPorDescricaoSemDiferenciarCaixa()
        {
            await SeedAsync();

            var resultado = await _repository.ListarAsync(_usuarioId, Filtro(busca: "MERCADO"));

            resultado.Itens.Should().ContainSingle().Which.Descricao.Should().Be("Supermercado");
        }

        [Fact]
        public async Task ListarAsync_PaginaCorretamente()
        {
            await SeedAsync();

            var pagina2 = await _repository.ListarAsync(_usuarioId, Filtro(pagina: 2, tamanho: 3));

            pagina2.Total.Should().Be(4);
            pagina2.TotalPaginas.Should().Be(2);
            pagina2.Itens.Should().ContainSingle().Which.Descricao.Should().Be("Salário");
        }

        [Fact]
        public async Task GetByIdAsync_DeOutroUsuario_RetornaNull()
        {
            await SeedAsync();
            var alheia = await _context.Transacoes.AsNoTracking().FirstAsync(t => t.UsuarioId == _outroUsuarioId);

            var resultado = await _repository.GetByIdAsync(alheia.Id, _usuarioId);

            resultado.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_PersisteAlteracoes()
        {
            await SeedAsync();
            var transacao = await _context.Transacoes.FirstAsync(t => t.Descricao == "Padaria");
            transacao.Atualizar("Padaria nova", "Alimentação", 25m, TipoTransacao.Despesa, Dia(15));

            await _repository.UpdateAsync(transacao);

            var salva = await _context.Transacoes.AsNoTracking().FirstAsync(t => t.Id == transacao.Id);
            salva.Descricao.Should().Be("Padaria nova");
            salva.Valor.Should().Be(25m);
        }
    }
}
