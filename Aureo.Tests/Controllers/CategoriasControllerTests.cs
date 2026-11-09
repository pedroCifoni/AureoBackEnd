using Aureo.Api.Controllers;
using Aureo.Application.DTOs.Categoria;
using Aureo.Application.Interfaces;
using Aureo.Domain.Entities;
using Aureo.Domain.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace Aureo.Tests.Controllers
{
    public class CategoriasControllerTests
    {
        private readonly Mock<ICategoriaService> _serviceMock = new();
        private readonly CategoriasController _controller;
        private readonly Guid _usuarioId = Guid.NewGuid();

        public CategoriasControllerTests()
        {
            _controller = new CategoriasController(_serviceMock.Object);

            var identity = new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, _usuarioId.ToString())], "Test");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        private static CategoriaResponseDto Resposta() =>
            new(Guid.NewGuid(), "Mercado", TipoTransacao.Despesa, "#6B7280", null);

        [Fact]
        public async Task Listar_DeveRetornar200()
        {
            IReadOnlyList<CategoriaResponseDto> lista = [Resposta()];
            _serviceMock.Setup(s => s.ListarAsync(_usuarioId, TipoTransacao.Despesa, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lista);

            var result = await _controller.Listar(TipoTransacao.Despesa, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(lista);
        }

        [Fact]
        public async Task Criar_DeveRetornar201()
        {
            var dto = new CriarCategoriaDto("Mercado", TipoTransacao.Despesa, null, null);
            var resposta = Resposta();
            _serviceMock.Setup(s => s.CriarAsync(_usuarioId, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resposta);

            var result = await _controller.Criar(dto, CancellationToken.None);

            var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            created.StatusCode.Should().Be(201);
            created.Value.Should().BeEquivalentTo(resposta);
        }

        [Fact]
        public async Task Atualizar_DeveRetornar200()
        {
            var id = Guid.NewGuid();
            var dto = new AtualizarCategoriaDto("Mercado", null, null);
            var resposta = Resposta();
            _serviceMock.Setup(s => s.AtualizarAsync(id, _usuarioId, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resposta);

            var result = await _controller.Atualizar(id, dto, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(resposta);
        }

        [Fact]
        public async Task Deletar_DeveRetornar204()
        {
            var id = Guid.NewGuid();

            var result = await _controller.Deletar(id, CancellationToken.None);

            result.Should().BeOfType<NoContentResult>();
            _serviceMock.Verify(s => s.DeletarAsync(id, _usuarioId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Deletar_EmUso_DevePropagarConflito()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.DeletarAsync(id, _usuarioId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ConflitoException("Categoria em uso."));

            var act = async () => await _controller.Deletar(id, CancellationToken.None);

            // GlobalExceptionHandler converte para 409.
            await act.Should().ThrowAsync<ConflitoException>();
        }
    }
}
