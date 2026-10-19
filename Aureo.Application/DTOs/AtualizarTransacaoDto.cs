using Aureo.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Aureo.Application.DTOs.Transacao
{
    public record AtualizarTransacaoDto(
        [property: Required, StringLength(200)] string Descricao,
        [property: Required, StringLength(100)] string Categoria,
        [property: Range(typeof(decimal), "0.01", "9999999999999999.99")] decimal Valor,
        [property: EnumDataType(typeof(TipoTransacao))] TipoTransacao Tipo,
        DateTime Data
    );
}
