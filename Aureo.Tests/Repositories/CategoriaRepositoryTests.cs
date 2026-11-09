using Aureo.Domain.Entities;
using Aureo.Infrastructure.Context;
using Aureo.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Aureo.Tests.Repositories
{
    public class CategoriaRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly CategoriaRepository _repository;
        private readonly Guid _usuarioId = Guid.NewGuid();
        private readonly Guid _outroUsuarioId = Guid.NewGuid();

        public CategoriaRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new CategoriaRepository(_context);
        }

        public void Dispose() => _context.Dispose();

        private static Categoria Nova(Guid usuarioId, string nome, TipoTransacao tipo) =>
            Categoria.Criar(nome, tipo, null, null, usuarioId);

        [Fact]
        public async Task ListarAsync_RetornaSomenteDoUsuarioOrdenadoPorTipoENome()
        {
            await _repository.AddRangeAsync([
                Nova(_usuarioId, "Zebra", TipoTransacao.Despesa),
                Nova(_usuarioId, "Alfa", TipoTransacao.Despesa),
                Nova(_usuarioId, "Salário", TipoTransacao.Receita),
                Nova(_outroUsuarioId, "Alheia", TipoTransacao.Despesa)
            ]);

            var resultado = await _repository.ListarAsync(_usuarioId, null);

            resultado.Select(c => c.Nome).Should().ContainInOrder("Salário", "Alfa", "Zebra");
            resultado.Should().NotContain(c => c.Nome == "Alheia");
        }

        [Fact]
        public async Task ListarAsync_FiltraPorTipo()
        {
            await _repository.AddRangeAsync([
                Nova(_usuarioId, "Salário", TipoTransacao.Receita),
                Nova(_usuarioId, "Lazer", TipoTransacao.Despesa)
            ]);

            var resultado = await _repository.ListarAsync(_usuarioId, TipoTransacao.Receita);

            resultado.Should().ContainSingle().Which.Nome.Should().Be("Salário");
        }

        [Fact]
        public async Task GetByIdAsync_DeOutroUsuario_RetornaNull()
        {
            var alheia = Nova(_outroUsuarioId, "Alheia", TipoTransacao.Despesa);
            await _repository.AddAsync(alheia);

            var resultado = await _repository.GetByIdAsync(alheia.Id, _usuarioId);

            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ExisteNomeAsync_IgnoraCaixaEConsideraTipoEUsuario()
        {
            var existente = Nova(_usuarioId, "Mercado", TipoTransacao.Despesa);
            await _repository.AddRangeAsync([existente, Nova(_outroUsuarioId, "Lazer", TipoTransacao.Despesa)]);

            (await _repository.ExisteNomeAsync(_usuarioId, "MERCADO", TipoTransacao.Despesa, null)).Should().BeTrue();
            (await _repository.ExisteNomeAsync(_usuarioId, "Mercado", TipoTransacao.Receita, null)).Should().BeFalse();
            (await _repository.ExisteNomeAsync(_usuarioId, "Lazer", TipoTransacao.Despesa, null)).Should().BeFalse();
            (await _repository.ExisteNomeAsync(_usuarioId, "Mercado", TipoTransacao.Despesa, existente.Id)).Should().BeFalse();
        }

        [Fact]
        public async Task EmUsoAsync_ReflitaExistenciaDeTransacoes()
        {
            var usada = Nova(_usuarioId, "Mercado", TipoTransacao.Despesa);
            var livre = Nova(_usuarioId, "Lazer", TipoTransacao.Despesa);
            await _repository.AddRangeAsync([usada, livre]);
            _context.Transacoes.Add(Transacao.Criar("Feira", usada, 10m, TipoTransacao.Despesa, DateTime.UtcNow, _usuarioId));
            await _context.SaveChangesAsync();

            (await _repository.EmUsoAsync(usada.Id)).Should().BeTrue();
            (await _repository.EmUsoAsync(livre.Id)).Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_PersisteAlteracoes()
        {
            var categoria = Nova(_usuarioId, "Mercado", TipoTransacao.Despesa);
            await _repository.AddAsync(categoria);

            var rastreada = await _repository.GetByIdAsync(categoria.Id, _usuarioId);
            rastreada!.Atualizar("Supermercado", "#000000", null);
            await _repository.UpdateAsync(rastreada);

            var salva = await _context.Categorias.AsNoTracking().FirstAsync(c => c.Id == categoria.Id);
            salva.Nome.Should().Be("Supermercado");
        }

        [Fact]
        public async Task DeleteAsync_Remove()
        {
            var categoria = Nova(_usuarioId, "Mercado", TipoTransacao.Despesa);
            await _repository.AddAsync(categoria);

            await _repository.DeleteAsync(categoria);

            (await _context.Categorias.CountAsync()).Should().Be(0);
        }
    }
}
