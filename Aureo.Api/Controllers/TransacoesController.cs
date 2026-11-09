using Aureo.Application.DTOs.Transacao;
using Aureo.Application.Interfaces;
using Aureo.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Listar([FromQuery] ListarTransacoesQuery query)
        {
            var resultado = await _transacaoService.ListarAsync(this.ObterUsuarioId(), query);
            return Ok(resultado);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            var transacao = await _transacaoService.ObterPorIdAsync(id, this.ObterUsuarioId());
            return Ok(transacao);
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarTransacaoDto dto)
        {
            var resultado = await _transacaoService.CriarAsync(this.ObterUsuarioId(), dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarTransacaoDto dto)
        {
            var resultado = await _transacaoService.AtualizarAsync(id, this.ObterUsuarioId(), dto);
            return Ok(resultado);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Deletar(Guid id)
        {
            await _transacaoService.DeletarAsync(id, this.ObterUsuarioId());
            return NoContent();
        }
    }
}
