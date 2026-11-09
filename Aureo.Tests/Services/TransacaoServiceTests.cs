// Aureo.Tests/Services/TransacaoServiceTests.cs
using Aureo.Application.DTOs.Transacao;
using Aureo.Application.Services;
using Aureo.Domain.Common;
using Aureo.Domain.Entities;
using Aureo.Domain.Exceptions;
using Aureo.Domain.Filtros;
using Aureo.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Aureo.Tests.Services
{
    public class TransacaoServiceTests
    {
        private readonly Mock<ITransacaoRepository> _repositoryMock;
        private readonly Mock<ICategoriaRepository> _categoriaRepositoryMock;
        private readonly TransacaoService _service;
        private readonly Guid _usuarioId = Guid.NewGuid();
        private readonly Categoria _salario;
        private readonly Categoria _alimentacao;
        private readonly Categoria _lazer;

        public TransacaoServiceTests()
        {
            _repositoryMock = new Mock<ITransacaoRepository>();
            _categoriaRepositoryMock = new Mock<ICategoriaRepository>();
            _service = new TransacaoService(_repositoryMock.Object, _categoriaRepositoryMock.Object);

            _salario = Categoria.Criar("Salário", TipoTransacao.Receita, null, null, _usuarioId);
            _alimentacao = Categoria.Criar("Alimentação", TipoTransacao.Despesa, null, null, _usuarioId);
            _lazer = Categoria.Criar("Lazer", TipoTransacao.Despesa, null, null, _usuarioId);

            foreach (var categoria in new[] { _salario, _alimentacao, _lazer })
            {
                var c = categoria;
                _categoriaRepositoryMock
                    .Setup(r => r.GetByIdAsync(c.Id, _usuarioId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(c);
            }
        }

        [Fact]
        public async Task CriarAsync_DeveCriarTransacaoERetornarDto()
        {
            // Arrange
            var dto = new CriarTransacaoDto(
                Descricao: "Salário",
                CategoriaId: _salario.Id,
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
            resultado.CategoriaId.Should().Be(_salario.Id);
            resultado.Categoria.Should().Be("Salário");
            resultado.Valor.Should().Be(4500m);
            resultado.Tipo.Should().Be(TipoTransacao.Receita);

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Once);
        }

        [Fact]
        public async Task CriarAsync_CategoriaInexistenteOuDeOutroUsuario_DeveLancarDominio()
        {
            var dto = new CriarTransacaoDto("x", Guid.NewGuid(), 10m, TipoTransacao.Despesa, DateTime.UtcNow);
            _categoriaRepositoryMock
                .Setup(r => r.GetByIdAsync(dto.CategoriaId, _usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Categoria?)null);

            var act = async () => await _service.CriarAsync(_usuarioId, dto);

            await act.Should().ThrowAsync<DominioException>().WithMessage("Categoria inválida.");
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Never);
        }

        [Fact]
        public async Task CriarAsync_TipoDivergenteDaCategoria_DeveLancarDominio()
        {
            var dto = new CriarTransacaoDto("x", _salario.Id, 10m, TipoTransacao.Despesa, DateTime.UtcNow);

            var act = async () => await _service.CriarAsync(_usuarioId, dto);

            await act.Should().ThrowAsync<DominioException>();
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Transacao>()), Times.Never);
        }

        [Fact]
        public async Task ListarAsync_DeveRepassarFiltroNormalizadoERetornarPagina()
        {
            // Arrange
            var transacoes = new List<Transacao>
            {
                Transacao.Criar("Salário", _salario, 4500m, TipoTransacao.Receita, DateTime.UtcNow, _usuarioId),
                Transacao.Criar("Supermercado", _alimentacao, 200m, TipoTransacao.Despesa, DateTime.UtcNow, _usuarioId)
            };
            TransacaoFiltro? capturado = null;

            _repositoryMock
                .Setup(r => r.ListarAsync(_usuarioId, It.IsAny<TransacaoFiltro>()))
                .Callback<Guid, TransacaoFiltro>((_, f) => capturado = f)
                .ReturnsAsync(new PagedResult<Transacao>(transacoes, 1, 20, 2));

            var query = new ListarTransacoesQuery
            {
                DataInicio = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                Tipo = TipoTransacao.Despesa,
                CategoriaId = _alimentacao.Id,
                Pagina = 1,
                Tamanho = 20
            };

            // Act
            var resultado = await _service.ListarAsync(_usuarioId, query);

            // Assert
            resultado.Total.Should().Be(2);
            resultado.Itens.Should().Contain(t => t.Descricao == "Salário" && t.Categoria == "Salário");
            resultado.Itens.Should().Contain(t => t.Descricao == "Supermercado" && t.Categoria == "Alimentação");
            capturado!.DataInicio!.Value.Kind.Should().Be(DateTimeKind.Utc);
            capturado.Tipo.Should().Be(TipoTransacao.Despesa);
            capturado.CategoriaId.Should().Be(_alimentacao.Id);
        }

        [Fact]
        public async Task AtualizarAsync_QuandoExiste_DeveAtualizarEPersistir()
        {
            var transacao = Transacao.Criar("Padaria", _alimentacao, 25m, TipoTransacao.Despesa, DateTime.UtcNow, _usuarioId);
            _repositoryMock.Setup(r => r.GetByIdAsync(transacao.Id, _usuarioId)).ReturnsAsync(transacao);
            _repositoryMock.Setup(r => r.UpdateAsync(transacao)).Returns(Task.CompletedTask);

            var dto = new AtualizarTransacaoDto("Padaria nova", _lazer.Id, 30m, TipoTransacao.Despesa, DateTime.UtcNow);

            var resultado = await _service.AtualizarAsync(transacao.Id, _usuarioId, dto);

            resultado.Descricao.Should().Be("Padaria nova");
            resultado.CategoriaId.Should().Be(_lazer.Id);
            resultado.Categoria.Should().Be("Lazer");
            resultado.Valor.Should().Be(30m);
            _repositoryMock.Verify(r => r.UpdateAsync(transacao), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_QuandoNaoExiste_DeveLancarKeyNotFound()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetByIdAsync(id, _usuarioId)).ReturnsAsync((Transacao?)null);

            var act = async () => await _service.AtualizarAsync(
                id, _usuarioId, new AtualizarTransacaoDto("x", _alimentacao.Id, 1m, TipoTransacao.Despesa, DateTime.UtcNow));

            await act.Should().ThrowAsync<KeyNotFoundException>();
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Transacao>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarAsync_CategoriaInvalida_DeveLancarDominioSemPersistir()
        {
            var transacao = Transacao.Criar("Padaria", _alimentacao, 25m, TipoTransacao.Despesa, DateTime.UtcNow, _usuarioId);
            _repositoryMock.Setup(r => r.GetByIdAsync(transacao.Id, _usuarioId)).ReturnsAsync(transacao);
            var categoriaAlheia = Guid.NewGuid();
            _categoriaRepositoryMock
                .Setup(r => r.GetByIdAsync(categoriaAlheia, _usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Categoria?)null);

            var act = async () => await _service.AtualizarAsync(
                transacao.Id, _usuarioId,
                new AtualizarTransacaoDto("Padaria", categoriaAlheia, 25m, TipoTransacao.Despesa, DateTime.UtcNow));

            await act.Should().ThrowAsync<DominioException>();
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Transacao>()), Times.Never);
        }

        [Fact]
        public async Task DeletarAsync_QuandoTransacaoExiste_DeveRemover()
        {
            // Arrange
            var transacao = Transacao.Criar("Padaria", _alimentacao, 25m, TipoTransacao.Despesa, DateTime.UtcNow, _usuarioId);

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
