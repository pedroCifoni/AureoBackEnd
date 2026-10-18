using Aureo.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aureo.Application.Interfaces
{
    public interface IUsuarioService
    {
        Task<CriarUsuarioResponseDto> CadastrarAsync(CriarUsuarioDto dto);
        public Task<LoginResponseDto> LoginAsync(LoginDto dto);
    }
}
