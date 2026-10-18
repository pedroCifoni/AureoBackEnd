using Aureo.Application.DTOs;
using Aureo.Application.Services;
using Aureo.Domain.Entities;
using Aureo.Domain.Exceptions;
using Aureo.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Cryptography;
using System.Text;

namespace Aureo.Tests.Services
{
    public class UsuarioServiceTests
    {
        private readonly Mock<IUsuarioRepository> _repoMock = new();
        private readonly UsuarioService _service;

        public UsuarioServiceTests()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:SecretKey"] = new string('k', 40),
                    ["Jwt:Issuer"] = "Aureo",
                    ["Jwt:Audience"] = "Aureo.Client"
                })
                .Build();

            _service = new UsuarioService(_repoMock.Object, config);
        }

        [Fact]
        public async Task CadastrarAsync_DeveGravarHashQueNaoEhSha256()
        {
            Usuario? salvo = null;
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Usuario>()))
                .Callback<Usuario>(u => salvo = u).Returns(Task.CompletedTask);

            await _service.CadastrarAsync(new CriarUsuarioDto { Nome = "pedro", Email = "A@B.com", Senha = "senha-forte-1" });

            salvo.Should().NotBeNull();
            salvo!.Email.Should().Be("a@b.com");
            salvo.SenhaHash.Should().NotBe("senha-forte-1");
            salvo.SenhaHash.Length.Should().BeGreaterThan(64);
        }

        [Fact]
        public async Task CadastrarAsync_NomeDuplicado_DeveLancarConflito()
        {
            _repoMock.Setup(r => r.ObterPorNomeAsync("pedro"))
                .ReturnsAsync(new Usuario("pedro", "x@y.com", "h"));

            var act = async () => await _service.CadastrarAsync(
                new CriarUsuarioDto { Nome = "pedro", Email = "a@b.com", Senha = "senha-forte-1" });

            await act.Should().ThrowAsync<ConflitoException>();
        }

        [Fact]
        public async Task CadastrarAsync_EmailDuplicado_DeveLancarConflito()
        {
            _repoMock.Setup(r => r.GetByEmailAsync("a@b.com"))
                .ReturnsAsync(new Usuario("outro", "a@b.com", "h"));

            var act = async () => await _service.CadastrarAsync(
                new CriarUsuarioDto { Nome = "pedro", Email = "A@B.com", Senha = "senha-forte-1" });

            await act.Should().ThrowAsync<ConflitoException>();
        }

        [Fact]
        public async Task LoginAsync_SenhaCorreta_DeveRetornarToken()
        {
            Usuario? salvo = null;
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Usuario>()))
                .Callback<Usuario>(u => salvo = u).Returns(Task.CompletedTask);
            await _service.CadastrarAsync(new CriarUsuarioDto { Nome = "pedro", Email = "a@b.com", Senha = "senha-forte-1" });
            _repoMock.Setup(r => r.ObterPorNomeAsync("pedro")).ReturnsAsync(salvo);

            var resultado = await _service.LoginAsync(new LoginDto { Nome = "pedro", Senha = "senha-forte-1" });

            resultado.Token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task LoginAsync_SenhaErrada_DeveLancarNaoAutorizado()
        {
            Usuario? salvo = null;
            _repoMock.Setup(r => r.AddAsync(It.IsAny<Usuario>()))
                .Callback<Usuario>(u => salvo = u).Returns(Task.CompletedTask);
            await _service.CadastrarAsync(new CriarUsuarioDto { Nome = "pedro", Email = "a@b.com", Senha = "senha-forte-1" });
            _repoMock.Setup(r => r.ObterPorNomeAsync("pedro")).ReturnsAsync(salvo);

            var act = async () => await _service.LoginAsync(new LoginDto { Nome = "pedro", Senha = "errada" });

            await act.Should().ThrowAsync<NaoAutorizadoException>();
        }

        [Fact]
        public async Task LoginAsync_HashLegadoSha256_DeveAceitarEMigrar()
        {
            var legado = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes("senha-antiga")));
            var usuario = new Usuario("pedro", "a@b.com", legado);
            _repoMock.Setup(r => r.ObterPorNomeAsync("pedro")).ReturnsAsync(usuario);
            _repoMock.Setup(r => r.UpdateAsync(usuario)).Returns(Task.CompletedTask);

            await _service.LoginAsync(new LoginDto { Nome = "pedro", Senha = "senha-antiga" });

            usuario.SenhaHash.Should().NotBe(legado);
            _repoMock.Verify(r => r.UpdateAsync(usuario), Times.Once);
        }
    }
}
