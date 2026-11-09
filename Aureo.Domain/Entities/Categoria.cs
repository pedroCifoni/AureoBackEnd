using System.Text.RegularExpressions;
using Aureo.Domain.Exceptions;

namespace Aureo.Domain.Entities
{
    public class Categoria
    {
        public const string CorPadrao = "#6B7280";

        private static readonly Regex CorHex = new("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

        public Guid Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public TipoTransacao Tipo { get; private set; }   // não muda depois de criada: as transações dependem dele
        public string Cor { get; private set; } = CorPadrao;
        public string? Icone { get; private set; }
        public Guid UsuarioId { get; private set; }

        private Categoria() { }

        public static Categoria Criar(
            string nome,
            TipoTransacao tipo,
            string? cor,
            string? icone,
            Guid usuarioId)
        {
            if (!Enum.IsDefined(tipo))
                throw new DominioException("Tipo de categoria inválido.");

            var categoria = new Categoria
            {
                Id = Guid.NewGuid(),
                Tipo = tipo,
                UsuarioId = usuarioId
            };
            categoria.Atualizar(nome, cor, icone);
            return categoria;
        }

        public void Atualizar(string nome, string? cor, string? icone)
        {
            if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length > 50)
                throw new DominioException("Nome da categoria é obrigatório (máx. 50 caracteres).");

            if (!string.IsNullOrWhiteSpace(cor) && !CorHex.IsMatch(cor.Trim()))
                throw new DominioException("Cor deve estar no formato #RRGGBB.");

            if (icone is { } i && i.Trim().Length > 50)
                throw new DominioException("Ícone deve ter no máximo 50 caracteres.");

            Nome = nome.Trim();
            Cor = string.IsNullOrWhiteSpace(cor) ? CorPadrao : cor.Trim().ToUpperInvariant();
            Icone = string.IsNullOrWhiteSpace(icone) ? null : icone.Trim();
        }

        public static IReadOnlyList<Categoria> CriarPadroes(Guid usuarioId) =>
        [
            Criar("Salário", TipoTransacao.Receita, "#16A34A", "briefcase", usuarioId),
            Criar("Investimentos", TipoTransacao.Receita, "#0EA5E9", "trending-up", usuarioId),
            Criar("Outras receitas", TipoTransacao.Receita, "#84CC16", "plus-circle", usuarioId),
            Criar("Alimentação", TipoTransacao.Despesa, "#F97316", "utensils", usuarioId),
            Criar("Moradia", TipoTransacao.Despesa, "#8B5CF6", "home", usuarioId),
            Criar("Transporte", TipoTransacao.Despesa, "#3B82F6", "car", usuarioId),
            Criar("Saúde", TipoTransacao.Despesa, "#EF4444", "heart-pulse", usuarioId),
            Criar("Lazer", TipoTransacao.Despesa, "#EC4899", "gamepad-2", usuarioId),
            Criar("Educação", TipoTransacao.Despesa, "#14B8A6", "graduation-cap", usuarioId),
            Criar("Outras despesas", TipoTransacao.Despesa, CorPadrao, "ellipsis", usuarioId)
        ];
    }
}
