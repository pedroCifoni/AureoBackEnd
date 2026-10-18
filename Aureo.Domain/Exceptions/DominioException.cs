namespace Aureo.Domain.Exceptions
{
    // Erro de regra de negócio / entrada inválida (400).
    public class DominioException : Exception
    {
        public DominioException(string mensagem) : base(mensagem) { }
    }

    // Recurso já existe (409).
    public class ConflitoException : DominioException
    {
        public ConflitoException(string mensagem) : base(mensagem) { }
    }

    // Credencial inválida ou usuário não autenticado (401).
    public class NaoAutorizadoException : DominioException
    {
        public NaoAutorizadoException(string mensagem) : base(mensagem) { }
    }
}
