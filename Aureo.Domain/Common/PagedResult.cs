namespace Aureo.Domain.Common
{
    public record PagedResult<T>(IReadOnlyList<T> Itens, int Pagina, int Tamanho, int Total)
    {
        public int TotalPaginas => Tamanho <= 0 ? 0 : (int)Math.Ceiling(Total / (double)Tamanho);

        public PagedResult<TOut> Map<TOut>(Func<T, TOut> map) =>
            new(Itens.Select(map).ToList(), Pagina, Tamanho, Total);
    }
}
