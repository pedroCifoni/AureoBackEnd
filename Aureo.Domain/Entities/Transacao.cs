using Aureo.Domain.Common;
using Aureo.Domain.Exceptions;

namespace Aureo.Domain.Entities
{
    public class Transacao
    {
        public Guid Id { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public Guid CategoriaId { get; private set; }
        public decimal Valor { get; private set; }        // sempre positivo; o sinal vem de Tipo
        public TipoTransacao Tipo { get; private set; }
        public DateTime Data { get; private set; }
        public Guid UsuarioId { get; private set; }

        public Usuario Usuario { get; private set; } = null!;
        public Categoria Categoria { get; private set; } = null!;

        private Transacao() { }

        public static Transacao Criar(
            string descricao,
            Categoria categoria,
            decimal valor,
            TipoTransacao tipo,
            DateTime data,
            Guid usuarioId)
        {
            var transacao = new Transacao
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId
            };
            transacao.Atualizar(descricao, categoria, valor, tipo, data);
            return transacao;
        }

        public void Atualizar(
            string descricao,
            Categoria categoria,
            decimal valor,
            TipoTransacao tipo,
            DateTime data)
        {
            if (string.IsNullOrWhiteSpace(descricao) || descricao.Trim().Length > 200)
                throw new DominioException("Descrição é obrigatória (máx. 200 caracteres).");

            if (categoria is null)
                throw new DominioException("Categoria é obrigatória.");

            if (categoria.UsuarioId != UsuarioId)
                throw new DominioException("Categoria inválida.");

            if (valor <= 0)
                throw new DominioException("Valor deve ser maior que zero.");

            if (!Enum.IsDefined(tipo))
                throw new DominioException("Tipo de transação inválido.");

            if (categoria.Tipo != tipo)
                throw new DominioException("O tipo da categoria não corresponde ao tipo da transação.");

            if (data == default)
                throw new DominioException("Data é obrigatória.");

            Descricao = descricao.Trim();
            Categoria = categoria;
            CategoriaId = categoria.Id;
            Valor = valor;
            Tipo = tipo;
            Data = DataUtc.Normalizar(data);
        }
    }

    public enum TipoTransacao
    {
        Receita,
        Despesa
    }
}
