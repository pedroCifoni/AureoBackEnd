using Aureo.Application.DTOs.Admin;
using Aureo.Application.Interfaces;
using Aureo.Domain.Common;
using Aureo.Domain.Entities;
using Aureo.Domain.Exceptions;
using Aureo.Domain.Filtros;
using Aureo.Domain.Interfaces;

namespace Aureo.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITransacaoRepository _transacaoRepository;

        public AdminService(IUsuarioRepository usuarioRepository, ITransacaoRepository transacaoRepository)
        {
            _usuarioRepository = usuarioRepository;
            _transacaoRepository = transacaoRepository;
        }

        public async Task<PagedResult<UsuarioAdminResponseDto>> ListarUsuariosAsync(
            ListarUsuariosQuery query, CancellationToken ct = default)
        {
            var pagina = await _usuarioRepository.ListarAsync(query.Busca, query.Pagina, query.Tamanho, ct);
            return pagina.Map(ToDto);
        }

        public async Task<UsuarioAdminResponseDto> ObterUsuarioAsync(Guid id, CancellationToken ct = default)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Usuário não encontrado.");

            return ToDto(usuario);
        }

        public async Task<UsuarioAdminResponseDto> AtualizarUsuarioAsync(
            Guid adminId, Guid id, AtualizarUsuarioAdminDto dto, CancellationToken ct = default)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Usuário não encontrado.");

            var perdeAdmin = usuario.Papel == PapelUsuario.Admin && dto.Papel != PapelUsuario.Admin;
            if (perdeAdmin)
                await GarantirOutroAdminAsync(adminId, usuario, "Você não pode remover o próprio papel de administrador.", ct);

            usuario.AtualizarDados(dto.Nome, dto.Email, dto.Papel);

            var comMesmoNome = await _usuarioRepository.ObterPorNomeAsync(usuario.Nome);
            if (comMesmoNome is not null && comMesmoNome.Id != usuario.Id)
                throw new ConflitoException("Este login já está em uso.");

            var comMesmoEmail = await _usuarioRepository.GetByEmailAsync(usuario.Email);
            if (comMesmoEmail is not null && comMesmoEmail.Id != usuario.Id)
                throw new ConflitoException("Este e-mail já está em uso.");

            await _usuarioRepository.UpdateAsync(usuario);
            return ToDto(usuario);
        }

        public async Task ExcluirUsuarioAsync(Guid adminId, Guid id, CancellationToken ct = default)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException("Usuário não encontrado.");

            if (usuario.Papel == PapelUsuario.Admin)
                await GarantirOutroAdminAsync(adminId, usuario, "Você não pode excluir a própria conta.", ct);

            await _usuarioRepository.DeleteAsync(usuario, ct);
        }

        public async Task<PagedResult<TransacaoAdminResponseDto>> ListarTransacoesAsync(
            ListarTransacoesAdminQuery query, CancellationToken ct = default)
        {
            var filtro = new TransacaoFiltro(
                query.DataInicio is { } inicio ? DataUtc.Normalizar(inicio) : null,
                query.DataFim is { } fim ? DataUtc.Normalizar(fim) : null,
                query.Tipo,
                query.CategoriaId,
                query.Busca,
                query.OrdenarPor,
                query.Decrescente,
                query.Pagina,
                query.Tamanho);

            var pagina = await _transacaoRepository.ListarTodasAsync(query.UsuarioId, filtro, ct);
            return pagina.Map(t => new TransacaoAdminResponseDto(
                t.Id,
                t.UsuarioId,
                t.Usuario.Nome,
                t.Descricao,
                t.CategoriaId,
                t.Categoria.Nome,
                t.Valor,
                t.Tipo,
                t.Data));
        }

        // Nunca deixa o sistema sem administrador, nem permite o admin se derrubar sozinho.
        private async Task GarantirOutroAdminAsync(
            Guid adminId, Usuario alvo, string mensagemAutoAcao, CancellationToken ct)
        {
            if (alvo.Id == adminId)
                throw new ConflitoException(mensagemAutoAcao);

            if (await _usuarioRepository.ContarAdminsAsync(ct) <= 1)
                throw new ConflitoException("É preciso manter ao menos um administrador.");
        }

        private static UsuarioAdminResponseDto ToDto(Usuario u) => new(u.Id, u.Nome, u.Email, u.Papel, u.CriadoEm);
    }
}
