using Aureo.Application.DTOs.Categoria;
using Aureo.Application.Services;
using Aureo.Domain.Entities;
using Aureo.Domain.Exceptions;
using Aureo.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Aureo.Tests.Services
{
    public class CategoriaServiceTests
    {
        private readonly Mock<ICategoriaRepository> _repoMock = new();
        private readonly CategoriaService _service;
        private readonly Guid _usuarioId = Guid.NewGuid();

        public CategoriaServiceTests()
        {
            _service = new CategoriaService(_repoMock.Object);
        }

        private Categoria Existente() =>
            Categoria.Criar("Mercado", TipoTransacao.Despesa, null, null, _usuarioId);

        [Fact]
        public async Task CriarAsync_DevePersistirERetornarDto()
        {
            var dto = new CriarCategoriaDto("Mercado", TipoTransacao.Despesa, "#112233", "cart");

            var resultado = await _service.CriarAsync(_usuarioId, dto);

            resultado.Nome.Should().Be("Mercado");
            resultado.Cor.Should().Be("#112233");
            _repoMock.Verify(r => r.AddAsync(
                It.Is<Categoria>(c => c.UsuarioId == _usuarioId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CriarAsync_NomeDuplicado_DeveLancarConflito()
        {
            _repoMock.Setup(r => r.ExisteNomeAsync(
                    _usuarioId, "Mercado", TipoTransacao.Despesa, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var act = async () => await _service.CriarAsync(
                _usuarioId, new CriarCategoriaDto("Mercado", TipoTransacao.Despesa, null, null));

            await act.Should().ThrowAsync<ConflitoException>();
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Categoria>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ListarAsync_DeveRepassarFiltroDeTipo()
        {
            _repoMock.Setup(r => r.ListarAsync(_usuarioId, TipoTransacao.Receita, It.IsAny<CancellationToken>()))
                .ReturnsAsync([Categoria.Criar("Salário", TipoTransacao.Receita, null, null, _usuarioId)]);

            var resultado = await _service.ListarAsync(_usuarioId, TipoTransacao.Receita);

            resultado.Should().ContainSingle().Which.Nome.Should().Be("Salário");
        }

        [Fact]
        public async Task ObterPorIdAsync_QuandoNaoExiste_DeveLancarKeyNotFound()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(id, _usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Categoria?)null);

            var act = async () => await _service.ObterPorIdAsync(id, _usuarioId);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task AtualizarAsync_DeveAlterarEPersistir()
        {
            var categoria = Existente();
            _repoMock.Setup(r => r.GetByIdAsync(categoria.Id, _usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoria);

            var resultado = await _service.AtualizarAsync(
                categoria.Id, _usuarioId, new AtualizarCategoriaDto("Supermercado", "#000000", null));

            resultado.Nome.Should().Be("Supermercado");
            resultado.Tipo.Should().Be(TipoTransacao.Despesa);
            _repoMock.Verify(r => r.UpdateAsync(categoria, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_NomeDeOutraCategoria_DeveLancarConflito()
        {
            var categoria = Existente();
            _repoMock.Setup(r => r.GetByIdAsync(categoria.Id, _usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoria);
            _repoMock.Setup(r => r.ExisteNomeAsync(
                    _usuarioId, "Lazer", TipoTransacao.Despesa, categoria.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var act = async () => await _service.AtualizarAsync(
                categoria.Id, _usuarioId, new AtualizarCategoriaDto("Lazer", null, null));

            await act.Should().ThrowAsync<ConflitoException>();
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Categoria>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeletarAsync_EmUso_DeveLancarConflito()
        {
            var categoria = Existente();
            _repoMock.Setup(r => r.GetByIdAsync(categoria.Id, _usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoria);
            _repoMock.Setup(r => r.EmUsoAsync(categoria.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var act = async () => await _service.DeletarAsync(categoria.Id, _usuarioId);

            await act.Should().ThrowAsync<ConflitoException>();
            _repoMock.Verify(r => r.DeleteAsync(It.IsAny<Categoria>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeletarAsync_SemUso_DeveRemover()
        {
            var categoria = Existente();
            _repoMock.Setup(r => r.GetByIdAsync(categoria.Id, _usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoria);

            await _service.DeletarAsync(categoria.Id, _usuarioId);

            _repoMock.Verify(r => r.DeleteAsync(categoria, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeletarAsync_QuandoNaoExiste_DeveLancarKeyNotFound()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(id, _usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Categoria?)null);

            var act = async () => await _service.DeletarAsync(id, _usuarioId);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }
    }
}
