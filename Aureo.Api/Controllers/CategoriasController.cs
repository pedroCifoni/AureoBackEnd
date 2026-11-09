using Aureo.Api.Extensions;
using Aureo.Application.DTOs.Categoria;
using Aureo.Application.Interfaces;
using Aureo.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aureo.Api.Controllers
{
    [ApiController]
    [Route("api/categorias")]
    [Authorize]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriasController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] TipoTransacao? tipo, CancellationToken ct)
        {
            var resultado = await _categoriaService.ListarAsync(this.ObterUsuarioId(), tipo, ct);
            return Ok(resultado);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
        {
            var categoria = await _categoriaService.ObterPorIdAsync(id, this.ObterUsuarioId(), ct);
            return Ok(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarCategoriaDto dto, CancellationToken ct)
        {
            var resultado = await _categoriaService.CriarAsync(this.ObterUsuarioId(), dto, ct);
            return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, resultado);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarCategoriaDto dto, CancellationToken ct)
        {
            var resultado = await _categoriaService.AtualizarAsync(id, this.ObterUsuarioId(), dto, ct);
            return Ok(resultado);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Deletar(Guid id, CancellationToken ct)
        {
            await _categoriaService.DeletarAsync(id, this.ObterUsuarioId(), ct);
            return NoContent();
        }
    }
}
