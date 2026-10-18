using Aureo.Application.DTOs;
using Aureo.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Aureo.Api.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar([FromBody] CriarUsuarioDto dto)
        {
            var resultado = await _usuarioService.CadastrarAsync(dto);
            return StatusCode(StatusCodes.Status201Created, resultado);
        }

        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _usuarioService.LoginAsync(dto);
            return Ok(result);
        }
    }
}
