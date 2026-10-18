using Aureo.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Aureo.Api.ExceptionHandling
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (status, mensagem) = exception switch
            {
                NaoAutorizadoException e => (StatusCodes.Status401Unauthorized, e.Message),
                ConflitoException e => (StatusCodes.Status409Conflict, e.Message),
                DominioException e => (StatusCodes.Status400BadRequest, e.Message),
                KeyNotFoundException e => (StatusCodes.Status404NotFound, e.Message),
                // Violação de índice único (corrida entre duas requisições de cadastro).
                DbUpdateException { InnerException: PostgresException { SqlState: "23505" } }
                    => (StatusCodes.Status409Conflict, "Registro já existe."),
                _ => (StatusCodes.Status500InternalServerError, "Erro interno.")
            };

            if (status == StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Erro não tratado em {Path}", httpContext.Request.Path);

            var problem = new ProblemDetails { Status = status, Title = mensagem };
            // Mantém o contrato { mensagem } usado pelo front.
            problem.Extensions["mensagem"] = mensagem;

            httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }
    }
}
