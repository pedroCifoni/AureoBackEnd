namespace Aureo.Domain.Entities
{
    public class Usuario
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string SenhaHash { get; private set; } = string.Empty;
        public DateTime CriadoEm { get; private set; }

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
    }
}
