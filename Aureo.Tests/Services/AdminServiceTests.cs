using Aureo.Application.DTOs.Admin;
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
    public class AdminServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarios = new();
        private readonly Mock<ITransacaoRepository> _transacoes = new();
        private readonly AdminService _service;
        private readonly Usuario _admin;
        private readonly Usuario _comum;

        public AdminServiceTests()
        {
            _service = new AdminService(_usuarios.Object, _transacoes.Object);

            _admin = new Usuario("admin", "admin@x.com", "h");
            _admin.AtualizarDados("admin", "admin@x.com", PapelUsuario.Admin);
            _comum = new Usuario("maria", "maria@x.com", "h");

            foreach (var u in new[] { _admin, _comum })
            {
                var usuario = u;
                _usuarios.Setup(r => r.GetByIdAsync(usuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);
            }
            _usuarios.Setup(r => r.ContarAdminsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(2);
        }

        [Fact]
        public async Task ListarUsuariosAsync_DeveMapearPagina()
        {
            _usuarios.Setup(r => r.ListarAsync("mar", 1, 20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<Usuario>([_comum], 1, 20, 1));

            var resultado = await _service.ListarUsuariosAsync(new ListarUsuariosQuery { Busca = "mar" });

            resultado.Total.Should().Be(1);
            resultado.Itens.Should().ContainSingle().Which.Email.Should().Be("maria@x.com");
        }

        [Fact]
        public async Task ObterUsuarioAsync_Inexistente_DeveLancarKeyNotFound()
        {
            var act = async () => await _service.ObterUsuarioAsync(Guid.NewGuid());

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task AtualizarUsuarioAsync_DevePersistirEPromover()
        {
            var dto = new AtualizarUsuarioAdminDto("maria2", "maria2@x.com", PapelUsuario.Admin);

            var resultado = await _service.AtualizarUsuarioAsync(_admin.Id, _comum.Id, dto);

            resultado.Nome.Should().Be("maria2");
            resultado.Papel.Should().Be(PapelUsuario.Admin);
            _usuarios.Verify(r => r.UpdateAsync(_comum), Times.Once);
        }

        [Fact]
        public async Task AtualizarUsuarioAsync_NomeDeOutroUsuario_DeveLancarConflito()
        {
            _usuarios.Setup(r => r.ObterPorNomeAsync("admin")).ReturnsAsync(_admin);

            var act = async () => await _service.AtualizarUsuarioAsync(
                _admin.Id, _comum.Id, new AtualizarUsuarioAdminDto("admin", "maria@x.com", PapelUsuario.Usuario));

            await act.Should().ThrowAsync<ConflitoException>();
            _usuarios.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarUsuarioAsync_EmailDeOutroUsuario_DeveLancarConflito()
        {
            _usuarios.Setup(r => r.GetByEmailAsync("admin@x.com")).ReturnsAsync(_admin);

            var act = async () => await _service.AtualizarUsuarioAsync(
                _admin.Id, _comum.Id, new AtualizarUsuarioAdminDto("maria", "admin@x.com", PapelUsuario.Usuario));

            await act.Should().ThrowAsync<ConflitoException>();
        }

        [Fact]
        public async Task AtualizarUsuarioAsync_AdminRebaixandoASiMesmo_DeveLancarConflito()
        {
            var act = async () => await _service.AtualizarUsuarioAsync(
                _admin.Id, _admin.Id, new AtualizarUsuarioAdminDto("admin", "admin@x.com", PapelUsuario.Usuario));

            await act.Should().ThrowAsync<ConflitoException>();
            _usuarios.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarUsuarioAsync_AdminEditandoSeusDadosMantendoPapel_DevePermitir()
        {
            var resultado = await _service.AtualizarUsuarioAsync(
                _admin.Id, _admin.Id, new AtualizarUsuarioAdminDto("admin2", "admin@x.com", PapelUsuario.Admin));

            resultado.Nome.Should().Be("admin2");
        }

        [Fact]
        public async Task AtualizarUsuarioAsync_RebaixarUltimoAdmin_DeveLancarConflito()
        {
            var outroAdmin = new Usuario("root", "root@x.com", "h");
            outroAdmin.AtualizarDados("root", "root@x.com", PapelUsuario.Admin);
            _usuarios.Setup(r => r.GetByIdAsync(outroAdmin.Id, It.IsAny<CancellationToken>())).ReturnsAsync(outroAdmin);
            _usuarios.Setup(r => r.ContarAdminsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var act = async () => await _service.AtualizarUsuarioAsync(
                _admin.Id, outroAdmin.Id, new AtualizarUsuarioAdminDto("root", "root@x.com", PapelUsuario.Usuario));

            await act.Should().ThrowAsync<ConflitoException>();
        }

        [Fact]
        public async Task ExcluirUsuarioAsync_Comum_DeveRemover()
        {
            await _service.ExcluirUsuarioAsync(_admin.Id, _comum.Id);

            _usuarios.Verify(r => r.DeleteAsync(_comum, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExcluirUsuarioAsync_AProprioConta_DeveLancarConflito()
        {
            var act = async () => await _service.ExcluirUsuarioAsync(_admin.Id, _admin.Id);

            await act.Should().ThrowAsync<ConflitoException>();
            _usuarios.Verify(r => r.DeleteAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ExcluirUsuarioAsync_Inexistente_DeveLancarKeyNotFound()
        {
            var act = async () => await _service.ExcluirUsuarioAsync(_admin.Id, Guid.NewGuid());

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task ListarTransacoesAsync_DeveRepassarFiltroENomeDoDono()
        {
            var categoria = Categoria.Criar("Lazer", TipoTransacao.Despesa, null, null, _comum.Id);
            var transacao = Transacao.Criar("Cinema", categoria, 60m, TipoTransacao.Despesa, DateTime.UtcNow, _comum.Id);
            typeof(Transacao).GetProperty(nameof(Transacao.Usuario))!.SetValue(transacao, _comum);

            TransacaoFiltro? capturado = null;
            Guid? usuarioCapturado = null;
            _transacoes
                .Setup(r => r.ListarTodasAsync(It.IsAny<Guid?>(), It.IsAny<TransacaoFiltro>(), It.IsAny<CancellationToken>()))
                .Callback<Guid?, TransacaoFiltro, CancellationToken>((u, f, _) => { usuarioCapturado = u; capturado = f; })
                .ReturnsAsync(new PagedResult<Transacao>([transacao], 1, 20, 1));

            var query = new ListarTransacoesAdminQuery
            {
                UsuarioId = _comum.Id,
                DataInicio = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
            };

            var resultado = await _service.ListarTransacoesAsync(query);

            usuarioCapturado.Should().Be(_comum.Id);
            capturado!.DataInicio!.Value.Kind.Should().Be(DateTimeKind.Utc);
            var item = resultado.Itens.Should().ContainSingle().Subject;
            item.UsuarioNome.Should().Be("maria");
            item.Categoria.Should().Be("Lazer");
        }
    }
}
