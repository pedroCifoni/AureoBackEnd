using Aureo.Domain.Entities;
using Aureo.Domain.Exceptions;
using FluentAssertions;

namespace Aureo.Tests.Domain
{
    public class CategoriaTests
    {
        private readonly Guid _usuarioId = Guid.NewGuid();

        [Fact]
        public void Criar_Valida_DeveNormalizarCampos()
        {
            var c = Categoria.Criar("  Mercado ", TipoTransacao.Despesa, "#abcdef", " cart ", _usuarioId);

            c.Nome.Should().Be("Mercado");
            c.Cor.Should().Be("#ABCDEF");
            c.Icone.Should().Be("cart");
            c.UsuarioId.Should().Be(_usuarioId);
            c.Id.Should().NotBeEmpty();
        }

        [Fact]
        public void Criar_SemCor_DeveUsarCorPadrao()
        {
            var c = Categoria.Criar("Mercado", TipoTransacao.Despesa, null, null, _usuarioId);

            c.Cor.Should().Be(Categoria.CorPadrao);
            c.Icone.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Criar_NomeVazio_DeveLancar(string nome)
        {
            var act = () => Categoria.Criar(nome, TipoTransacao.Despesa, null, null, _usuarioId);
            act.Should().Throw<DominioException>();
        }

        [Fact]
        public void Criar_NomeMuitoLongo_DeveLancar()
        {
            var act = () => Categoria.Criar(new string('a', 51), TipoTransacao.Despesa, null, null, _usuarioId);
            act.Should().Throw<DominioException>();
        }

        [Theory]
        [InlineData("vermelho")]
        [InlineData("#FFF")]
        [InlineData("#GGGGGG")]
        public void Criar_CorInvalida_DeveLancar(string cor)
        {
            var act = () => Categoria.Criar("x", TipoTransacao.Despesa, cor, null, _usuarioId);
            act.Should().Throw<DominioException>();
        }

        [Fact]
        public void Criar_TipoInvalido_DeveLancar()
        {
            var act = () => Categoria.Criar("x", (TipoTransacao)99, null, null, _usuarioId);
            act.Should().Throw<DominioException>();
        }

        [Fact]
        public void Atualizar_NaoAlteraTipo()
        {
            var c = Categoria.Criar("x", TipoTransacao.Receita, null, null, _usuarioId);

            c.Atualizar("y", "#000000", "i");

            c.Tipo.Should().Be(TipoTransacao.Receita);
            c.Nome.Should().Be("y");
        }

        [Fact]
        public void CriarPadroes_DeveTerReceitasEDespesasSemNomesRepetidos()
        {
            var padroes = Categoria.CriarPadroes(_usuarioId);

            padroes.Should().Contain(c => c.Tipo == TipoTransacao.Receita);
            padroes.Should().Contain(c => c.Tipo == TipoTransacao.Despesa);
            padroes.Should().OnlyContain(c => c.UsuarioId == _usuarioId);
            padroes.Select(c => (c.Nome, c.Tipo)).Should().OnlyHaveUniqueItems();
        }
    }
}
