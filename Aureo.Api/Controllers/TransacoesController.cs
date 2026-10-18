using Aureo.Application.DTOs.Transacao;
using Aureo.Application.Interfaces;
using Aureo.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aureo.Api.Controllers
{
    [ApiController]
    [Route("api/transacoes")]
    [Authorize]
    public class TransacoesController : ControllerBase
    {
        private readonly ITransacaoService _transacaoService;

        public TransacoesController(ITransacaoService transacaoService)
        {
            _transacaoService = transacaoService;
        }

        [HttpGet]
        public async Task<IActionResult> ObterTodas()
        {
            var transacoes = await _transacaoService.ObterTodasAsync(ObterUsuarioId());
            return Ok(transacoes);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            var transacao = await _transacaoService.ObterPorIdAsync(id, ObterUsuarioId());
            return Ok(transacao);
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarTransacaoDto dto)
        {
            var resultado = await _transacaoService.CriarAsync(ObterUsuarioId(), dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Deletar(Guid id)
        {
            await _transacaoService.DeletarAsync(id, ObterUsuarioId());
            return NoContent();
        }

        private Guid ObterUsuarioId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(claim, out var id)
                ? id
                : throw new NaoAutorizadoException("Usuário não autenticado.");
        }
    }
}
