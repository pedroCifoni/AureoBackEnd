using Aureo.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Aureo.Api.Extensions
{
    public static class ControllerBaseExtensions
    {
        // O id do usuário vem sempre do token, nunca do cliente.
        public static Guid ObterUsuarioId(this ControllerBase controller)
        {
            var claim = controller.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(claim, out var id)
                ? id
                : throw new NaoAutorizadoException("Usuário não autenticado.");
        }
    }
}
