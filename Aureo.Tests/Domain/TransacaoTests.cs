using Aureo.Domain.Entities;
using Aureo.Domain.Exceptions;
using FluentAssertions;

namespace Aureo.Tests.Domain
{
    public class TransacaoTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Criar_ValorNaoPositivo_DeveLancar(int valor)
        {
            var act = () => Transacao.Criar("x", "y", valor, TipoTransacao.Despesa, DateTime.UtcNow, Guid.NewGuid());
            act.Should().Throw<DominioException>();
        }

        [Fact]
        public void Criar_DescricaoVazia_DeveLancar()
        {
            var act = () => Transacao.Criar(" ", "y", 10, TipoTransacao.Despesa, DateTime.UtcNow, Guid.NewGuid());
            act.Should().Throw<DominioException>();
        }

        [Fact]
        public void Criar_DataUnspecified_DeveVirarUtc()
        {
            var data = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Unspecified);
            var t = Transacao.Criar("x", "y", 10, TipoTransacao.Receita, data, Guid.NewGuid());
            t.Data.Kind.Should().Be(DateTimeKind.Utc);
        }
    }
}
