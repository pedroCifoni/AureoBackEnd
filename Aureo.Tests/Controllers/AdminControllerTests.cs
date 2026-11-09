using Aureo.Api.Controllers;
using Aureo.Application.DTOs.Admin;
using Aureo.Application.Interfaces;
using Aureo.Domain.Common;
using Aureo.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using System.Security.Claims;

namespace Aureo.Tests.Controllers
{
    public class AdminControllerTests
    {
        private readonly Mock<IAdminService> _serviceMock = new();
        private readonly AdminController _controller;
        private readonly Guid _adminId = Guid.NewGuid();

        public AdminControllerTests()
        {
            _controller = new AdminController(_serviceMock.Object);
            var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, _adminId.ToString())], "Test");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        [Fact]
        public void Controller_ExigePapelAdmin()
        {
            var atributo = typeof(AdminController).GetCustomAttribute<AuthorizeAttribute>();

            atributo.Should().NotBeNull();
            atributo!.Roles.Should().Be("Admin");
        }

        [Fact]
        public async Task ListarUsuarios_DeveRetornar200()
        {
            var query = new ListarUsuariosQuery();
            var pagina = new PagedResult<UsuarioAdminResponseDto>([], 1, 20, 0);
            _serviceMock.Setup(s => s.ListarUsuariosAsync(query, It.IsAny<CancellationToken>())).ReturnsAsync(pagina);

            var result = await _controller.ListarUsuarios(query, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(pagina);
        }

        [Fact]
        public async Task AtualizarUsuario_DeveUsarIdDoAdminDoToken()
        {
            var id = Guid.NewGuid();
            var dto = new AtualizarUsuarioAdminDto("maria", "m@x.com", PapelUsuario.Usuario);
            var resposta = new UsuarioAdminResponseDto(id, "maria", "m@x.com", PapelUsuario.Usuario, DateTime.UtcNow);
            _serviceMock.Setup(s => s.AtualizarUsuarioAsync(_adminId, id, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resposta);

            var result = await _controller.AtualizarUsuario(id, dto, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(resposta);
        }

        [Fact]
        public async Task ExcluirUsuario_DeveRetornar204()
        {
            var id = Guid.NewGuid();

            var result = await _controller.ExcluirUsuario(id, CancellationToken.None);

            result.Should().BeOfType<NoContentResult>();
            _serviceMock.Verify(s => s.ExcluirUsuarioAsync(_adminId, id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ListarTransacoes_DeveRetornar200()
        {
            var query = new ListarTransacoesAdminQuery();
            var pagina = new PagedResult<TransacaoAdminResponseDto>([], 1, 20, 0);
            _serviceMock.Setup(s => s.ListarTransacoesAsync(query, It.IsAny<CancellationToken>())).ReturnsAsync(pagina);

            var result = await _controller.ListarTransacoes(query, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(pagina);
        }
    }
}
