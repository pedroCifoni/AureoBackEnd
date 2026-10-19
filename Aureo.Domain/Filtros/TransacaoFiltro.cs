using Aureo.Domain.Entities;

namespace Aureo.Domain.Filtros
{
    public enum OrdenacaoTransacao
    {
        Data,
        Valor
    }

    public record TransacaoFiltro(
        DateTime? DataInicio,
        DateTime? DataFim,
        TipoTransacao? Tipo,
        string? Categoria,
        string? Busca,
        OrdenacaoTransacao OrdenarPor,
        bool Decrescente,
        int Pagina,
        int Tamanho);
}
