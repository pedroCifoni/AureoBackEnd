// Aureo.Tests/Controllers/TransacoesControllerTests.cs
using Aureo.Application.DTOs.Transacao;
using Aureo.Application.Interfaces;
using Aureo.Api.Controllers;
using Aureo.Domain.Common;
using Aureo.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace Aureo.Tests.Controllers
{
    public class TransacoesControllerTests
    {
        private readonly Mock<ITransacaoService> _serviceMock;
        private readonly TransacoesController _controller;
        private readonly Guid _usuarioId = Guid.NewGuid();

        public TransacoesControllerTests()
        {
            _serviceMock = new Mock<ITransacaoService>();
            _controller = new TransacoesController(_serviceMock.Object);

            // Simula usuário autenticado no controller
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _usuarioId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task Listar_DeveRetornar200ComPagina()
        {
            // Arrange
            var itens = new List<TransacaoResponseDto>
            {
                new(Guid.NewGuid(), "Salário", "Receita", 4500m, TipoTransacao.Receita, DateTime.UtcNow),
                new(Guid.NewGuid(), "Supermercado", "Alimentação", 200m, TipoTransacao.Despesa, DateTime.UtcNow)
            };
            var pagina = new PagedResult<TransacaoResponseDto>(itens, 1, 20, 2);
            var query = new ListarTransacoesQuery();

            _serviceMock
                .Setup(s => s.ListarAsync(_usuarioId, query))
                .ReturnsAsync(pagina);

            // Act
            var result = await _controller.Listar(query);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.StatusCode.Should().Be(200);
            ok.Value.Should().BeEquivalentTo(pagina);
        }

        [Fact]
        public async Task Atualizar_DeveRetornar200ComTransacaoAtualizada()
        {
            var id = Guid.NewGuid();
            var dto = new AtualizarTransacaoDto("Salário", "Receita", 5000m, TipoTransacao.Receita, DateTime.UtcNow);
            var response = new TransacaoResponseDto(id, "Salário", "Receita", 5000m, TipoTransacao.Receita, dto.Data);

            _serviceMock.Setup(s => s.AtualizarAsync(id, _usuarioId, dto)).ReturnsAsync(response);

            var result = await _controller.Atualizar(id, dto);

            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task Criar_DeveRetornar201ComTransacaoCriada()
        {
            // Arrange
            var dto = new CriarTransacaoDto("Salário", "Receita", 4500m, TipoTransacao.Receita, DateTime.UtcNow);
            var response = new TransacaoResponseDto(Guid.NewGuid(), "Salário", "Receita", 4500m, TipoTransacao.Receita, DateTime.UtcNow);

            _serviceMock
                .Setup(s => s.CriarAsync(_usuarioId, dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Criar(dto);

            // Assert
            var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            created.StatusCode.Should().Be(201);
            created.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task Deletar_QuandoExiste_DeveRetornar204()
        {
            // Arrange
            var id = Guid.NewGuid();

            _serviceMock
                .Setup(s => s.DeletarAsync(id, _usuarioId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Deletar(id);

            // Assert
            result.Should().BeOfType<NoContentResult>()
                .Which.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task Deletar_QuandoNaoExiste_DevePropagarKeyNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            _serviceMock
                .Setup(s => s.DeletarAsync(id, _usuarioId))
                .ThrowsAsync(new KeyNotFoundException("Transação não encontrada."));

            // Act
            var act = async () => await _controller.Deletar(id);

            // Assert (GlobalExceptionHandler converte para 404)
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }
    }
}