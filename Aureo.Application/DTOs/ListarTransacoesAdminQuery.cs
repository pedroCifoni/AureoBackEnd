using Aureo.Application.DTOs.Transacao;

namespace Aureo.Application.DTOs.Admin
{
    // Mesmos filtros da listagem do usuário, mais o recorte por dono.
    public class ListarTransacoesAdminQuery : ListarTransacoesQuery
    {
        public Guid? UsuarioId { get; set; }
    }
}
