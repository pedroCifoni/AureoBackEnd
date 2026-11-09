using Aureo.Domain.Exceptions;

namespace Aureo.Domain.Entities
{
    public class Usuario
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string SenhaHash { get; private set; } = string.Empty;
        public DateTime CriadoEm { get; private set; }
        public PapelUsuario Papel { get; private set; } = PapelUsuario.Usuario;

        protected Usuario() { }

        public Usuario(string nome, string email, string senhaHash)
        {
            Id = Guid.NewGuid();
            Nome = nome;
            Email = email;
            SenhaHash = senhaHash;
            CriadoEm = DateTime.UtcNow;
        }

        public void AtualizarSenhaHash(string senhaHash) => SenhaHash = senhaHash;

        // Edição feita por um administrador; o cadastro continua validado na borda (DTO).
        public void AtualizarDados(string nome, string email, PapelUsuario papel)
        {
            if (string.IsNullOrWhiteSpace(nome) || nome.Trim().Length is < 3 or > 100)
                throw new DominioException("Nome deve ter de 3 a 100 caracteres.");

            var emailNormalizado = email?.Trim().ToLowerInvariant() ?? string.Empty;
            if (emailNormalizado.Length is 0 or > 150 || !emailNormalizado.Contains('@'))
                throw new DominioException("E-mail inválido.");

            if (!Enum.IsDefined(papel))
                throw new DominioException("Papel inválido.");

            Nome = nome.Trim();
            Email = emailNormalizado;
            Papel = papel;
        }
    }
}
