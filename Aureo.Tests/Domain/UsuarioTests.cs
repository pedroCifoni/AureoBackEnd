using Aureo.Domain.Entities;
using Aureo.Domain.Exceptions;
using FluentAssertions;

namespace Aureo.Tests.Domain
{
    public class UsuarioTests
    {
        private static Usuario Novo() => new("pedro", "a@b.com", "hash");

        [Fact]
        public void NovoUsuario_ComecaComoUsuarioComum()
        {
            Novo().Papel.Should().Be(PapelUsuario.Usuario);
        }

        [Fact]
        public void AtualizarDados_Valido_DeveNormalizar()
        {
            var u = Novo();

            u.AtualizarDados("  maria  ", " MARIA@X.com ", PapelUsuario.Admin);

            u.Nome.Should().Be("maria");
            u.Email.Should().Be("maria@x.com");
            u.Papel.Should().Be(PapelUsuario.Admin);
        }

        [Theory]
        [InlineData("ab", "a@b.com")]
        [InlineData("  ", "a@b.com")]
        [InlineData("pedro", "sem-arroba")]
        [InlineData("pedro", "")]
        public void AtualizarDados_Invalido_DeveLancar(string nome, string email)
        {
            var act = () => Novo().AtualizarDados(nome, email, PapelUsuario.Usuario);

            act.Should().Throw<DominioException>();
        }

        [Fact]
        public void AtualizarDados_PapelInvalido_DeveLancar()
        {
            var act = () => Novo().AtualizarDados("pedro", "a@b.com", (PapelUsuario)99);

            act.Should().Throw<DominioException>();
        }
    }
}
