using Aureo.Domain.Entities;
using Aureo.Domain.Exceptions;
using FluentAssertions;

namespace Aureo.Tests.Domain
{
    public class TransacaoTests
    {
        private readonly Guid _usuarioId = Guid.NewGuid();

        private Categoria CategoriaDespesa(Guid? usuarioId = null) =>
            Categoria.Criar("Alimentação", TipoTransacao.Despesa, null, null, usuarioId ?? _usuarioId);

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Criar_ValorNaoPositivo_DeveLancar(int valor)
        {
            var act = () => Transacao.Criar("x", CategoriaDespesa(), valor, TipoTransacao.Despesa, DateTime.UtcNow, _usuarioId);
            act.Should().Throw<DominioException>();
        }

        [Fact]
        public void Criar_DescricaoVazia_DeveLancar()
        {
            var act = () => Transacao.Criar(" ", CategoriaDespesa(), 10, TipoTransacao.Despesa, DateTime.UtcNow, _usuarioId);
            act.Should().Throw<DominioException>();
        }

        [Fact]
        public void Criar_DataUnspecified_DeveVirarUtc()
        {
            var categoria = Categoria.Criar("Salário", TipoTransacao.Receita, null, null, _usuarioId);
            var data = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Unspecified);

            var t = Transacao.Criar("x", categoria, 10, TipoTransacao.Receita, data, _usuarioId);

            t.Data.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public void Criar_DeveVincularCategoria()
        {
            var categoria = CategoriaDespesa();

            var t = Transacao.Criar("x", categoria, 10, TipoTransacao.Despesa, DateTime.UtcNow, _usuarioId);

            t.CategoriaId.Should().Be(categoria.Id);
            t.Categoria.Should().BeSameAs(categoria);
        }

        [Fact]
        public void Criar_CategoriaDeOutroUsuario_DeveLancar()
        {
            var categoriaAlheia = CategoriaDespesa(Guid.NewGuid());

            var act = () => Transacao.Criar("x", categoriaAlheia, 10, TipoTransacao.Despesa, DateTime.UtcNow, _usuarioId);

            act.Should().Throw<DominioException>().WithMessage("Categoria inválida.");
        }

        [Fact]
        public void Criar_TipoDivergenteDaCategoria_DeveLancar()
        {
            var act = () => Transacao.Criar("x", CategoriaDespesa(), 10, TipoTransacao.Receita, DateTime.UtcNow, _usuarioId);

            act.Should().Throw<DominioException>();
        }

        [Fact]
        public void Criar_CategoriaNula_DeveLancar()
        {
            var act = () => Transacao.Criar("x", null!, 10, TipoTransacao.Despesa, DateTime.UtcNow, _usuarioId);

            act.Should().Throw<DominioException>();
        }
    }
}
