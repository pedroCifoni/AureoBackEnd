using Aureo.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Aureo.Application.DTOs.Admin
{
    public record UsuarioAdminResponseDto(
        Guid Id,
        string Nome,
        string Email,
        PapelUsuario Papel,
        DateTime CriadoEm
    );

    public record AtualizarUsuarioAdminDto(
        [property: Required, StringLength(100, MinimumLength = 3)] string Nome,
        [property: Required, EmailAddress, StringLength(150)] string Email,
        [property: EnumDataType(typeof(PapelUsuario))] PapelUsuario Papel
    );

    public class ListarUsuariosQuery
    {
        [StringLength(150)]
        public string? Busca { get; set; }

        [Range(1, int.MaxValue)]
        public int Pagina { get; set; } = 1;

        [Range(1, 100)]
        public int Tamanho { get; set; } = 20;
    }

    public record TransacaoAdminResponseDto(
        Guid Id,
        Guid UsuarioId,
        string UsuarioNome,
        string Descricao,
        Guid CategoriaId,
        string Categoria,
        decimal Valor,
        TipoTransacao Tipo,
        DateTime Data
    );
}
