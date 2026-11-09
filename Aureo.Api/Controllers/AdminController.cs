using Aureo.Api.Extensions;
using Aureo.Application.DTOs.Admin;
using Aureo.Application.Interfaces;
using Aureo.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aureo.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = nameof(PapelUsuario.Admin))]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("usuarios")]
        public async Task<IActionResult> ListarUsuarios([FromQuery] ListarUsuariosQuery query, CancellationToken ct)
        {
            return Ok(await _adminService.ListarUsuariosAsync(query, ct));
        }

        [HttpGet("usuarios/{id:guid}")]
        public async Task<IActionResult> ObterUsuario(Guid id, CancellationToken ct)
        {
            return Ok(await _adminService.ObterUsuarioAsync(id, ct));
        }

        [HttpPut("usuarios/{id:guid}")]
        public async Task<IActionResult> AtualizarUsuario(
            Guid id, [FromBody] AtualizarUsuarioAdminDto dto, CancellationToken ct)
        {
            return Ok(await _adminService.AtualizarUsuarioAsync(this.ObterUsuarioId(), id, dto, ct));
        }

        [HttpDelete("usuarios/{id:guid}")]
        public async Task<IActionResult> ExcluirUsuario(Guid id, CancellationToken ct)
        {
            await _adminService.ExcluirUsuarioAsync(this.ObterUsuarioId(), id, ct);
            return NoContent();
        }

        [HttpGet("transacoes")]
        public async Task<IActionResult> ListarTransacoes([FromQuery] ListarTransacoesAdminQuery query, CancellationToken ct)
        {
            return Ok(await _adminService.ListarTransacoesAsync(query, ct));
        }
    }
}
