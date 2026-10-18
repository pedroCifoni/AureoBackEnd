// Aureo.Tests/Services/TransacaoServiceTests.cs
using Aureo.Application.DTOs.Transacao;
using Aureo.Application.Services;
using Aureo.Domain.Entities;
using Aureo.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Aureo.Tests.Services
{
    public class TransacaoServiceTests
    {
        private readonly Mock<ITransacaoRepository> _repositoryMock;
        private readonly TransacaoService _service;
        private readonly Guid _usuarioId = Guid.NewGuid();

        public TransacaoServiceTests()
        {
            _repositoryMock = new Mock<ITransacaoRepository>();
            _service = new TransacaoService(_repositoryMock.Object);
        }

        [Fact]
        public async Task CriarAsync_DeveCriarTransacaoERetornarDto()
        {
            // Arrange
            var dto = new CriarTransacaoDto(
                Descricao: "Salário",
                Categoria: "Receita",
                Valor: 4500m,
                Tipo: TipoTransacao.Receita,
                Data: DateTime.UtcNow
            );

            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<Transacao>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.CriarAsync(_usuarioId, dto);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Descricao.Should().Be("Salário");
            resultado.Valor.Should().Be(4500m);
            resultado.Tipo.Should().Be(TipoTransacao.Receita);

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Once);
        }

        [Fact]
        public async Task ObterTodasAsync_DeveRetornarListaDeTransacoes()
        {
            // Arrange
            var transacoes = new List<Transacao>
            {
                Transacao.Criar("Salário", "Receita", 4500m, TipoTransacao.Receita, DateTime.UtcNow, _usuarioId),
                Transacao.Criar("Supermercado", "Alimentação", 200m, TipoTransacao.Despesa, DateTime.UtcNow, _usuarioId)
            };

            _repositoryMock
                .Setup(r => r.GetAllByUsuarioAsync(_usuarioId))
                .ReturnsAsync(transacoes);

            // Act
            var resultado = await _service.ObterTodasAsync(_usuarioId);

            // Assert
            resultado.Should().HaveCount(2);
            resultado.Should().Contain(t => t.Descricao == "Salário");
            resultado.Should().Contain(t => t.Descricao == "Supermercado");
        }

        [Fact]
        public async Task DeletarAsync_QuandoTransacaoExiste_DeveRemover()
        {
            // Arrange
            var transacao = Transacao.Criar("Padaria", "Alimentação", 25m, TipoTransacao.Despesa, DateTime.UtcNow, _usuarioId);

            _repositoryMock
                .Setup(r => r.GetByIdAsync(transacao.Id, _usuarioId))
                .ReturnsAsync(transacao);

            _repositoryMock
                .Setup(r => r.DeleteAsync(transacao))
                .Returns(Task.CompletedTask);

            // Act
            await _service.DeletarAsync(transacao.Id, _usuarioId);

            // Assert
            _repositoryMock.Verify(r => r.DeleteAsync(transacao), Times.Once);
        }

        [Fact]
        public async Task DeletarAsync_QuandoTransacaoNaoExiste_DeveLancarKeyNotFoundException()
        {
            // Arrange
            var idInexistente = Guid.NewGuid();

            _repositoryMock
                .Setup(r => r.GetByIdAsync(idInexistente, _usuarioId))
                .ReturnsAsync((Transacao?)null);

            // Act
            var act = async () => await _service.DeletarAsync(idInexistente, _usuarioId);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Transação não encontrada.");
        }
    }
}