using Aureo.Domain.Entities;
using Aureo.Domain.Filtros;
using System.ComponentModel.DataAnnotations;

namespace Aureo.Application.DTOs.Transacao
{
    public class ListarTransacoesQuery : IValidatableObject
    {
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public TipoTransacao? Tipo { get; set; }

        [StringLength(100)]
        public string? Categoria { get; set; }

        [StringLength(200)]
        public string? Busca { get; set; }

        public OrdenacaoTransacao OrdenarPor { get; set; } = OrdenacaoTransacao.Data;
        public bool Decrescente { get; set; } = true;

        [Range(1, int.MaxValue)]
        public int Pagina { get; set; } = 1;

        [Range(1, 100)]
        public int Tamanho { get; set; } = 20;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DataInicio.HasValue && DataFim.HasValue && DataFim < DataInicio)
                yield return new ValidationResult(
                    "DataFim deve ser maior ou igual a DataInicio.", [nameof(DataFim)]);
        }
    }
}
