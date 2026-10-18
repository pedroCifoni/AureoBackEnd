using Aureo.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aureo.Application.DTOs.Transacao
{
    public record TransacaoResponseDto(
        Guid Id,
        string Descricao,
        string Categoria,
        decimal Valor,
        TipoTransacao Tipo,
        DateTime Data
    );
}
