using Aureo.Application.DTOs.Admin;
using Aureo.Domain.Common;

namespace Aureo.Application.Interfaces
{
    public interface IAdminService
    {
        Task<PagedResult<UsuarioAdminResponseDto>> ListarUsuariosAsync(ListarUsuariosQuery query, CancellationToken ct = default);
        Task<UsuarioAdminResponseDto> ObterUsuarioAsync(Guid id, CancellationToken ct = default);
        Task<UsuarioAdminResponseDto> AtualizarUsuarioAsync(Guid adminId, Guid id, AtualizarUsuarioAdminDto dto, CancellationToken ct = default);
        Task ExcluirUsuarioAsync(Guid adminId, Guid id, CancellationToken ct = default);
        Task<PagedResult<TransacaoAdminResponseDto>> ListarTransacoesAsync(ListarTransacoesAdminQuery query, CancellationToken ct = default);
    }
}
