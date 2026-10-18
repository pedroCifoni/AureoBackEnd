using Aureo.Application.DTOs;
using Aureo.Application.Interfaces;
using Aureo.Domain.Entities;
using Aureo.Domain.Exceptions;
using Aureo.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Aureo.Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<Usuario> _hasher = new();

        public UsuarioService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
        }

        public async Task<CriarUsuarioResponseDto> CadastrarAsync(CriarUsuarioDto dto)
        {
            var nome = dto.Nome.Trim();
            var email = dto.Email.Trim().ToLowerInvariant();

            if (await _usuarioRepository.ObterPorNomeAsync(nome) != null)
                throw new ConflitoException("Este login já está em uso.");

            if (await _usuarioRepository.GetByEmailAsync(email) != null)
                throw new ConflitoException("Este e-mail já está em uso.");

            var novoUsuario = new Usuario(nome, email, string.Empty);
            novoUsuario.AtualizarSenhaHash(_hasher.HashPassword(novoUsuario, dto.Senha));

            await _usuarioRepository.AddAsync(novoUsuario);

            return new CriarUsuarioResponseDto
            {
                Id = novoUsuario.Id,
                Nome = novoUsuario.Nome,
                Email = novoUsuario.Email,
                CriadoEm = novoUsuario.CriadoEm
            };
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var usuario = await _usuarioRepository.ObterPorNomeAsync(dto.Nome.Trim());

            if (usuario == null || !await VerificarSenhaAsync(usuario, dto.Senha))
                throw new NaoAutorizadoException("Usuário ou senha inválidos.");

            return new LoginResponseDto
            {
                Nome = usuario.Nome,
                Email = usuario.Email,
                Token = GerarTokenJwt(usuario)
            };
        }

        private async Task<bool> VerificarSenhaAsync(Usuario usuario, string senha)
        {
            // Hashes antigos (SHA256 hex, 64 chars) migram para PasswordHasher no primeiro login.
            if (EhHashLegado(usuario.SenhaHash))
            {
                var igual = CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(usuario.SenhaHash),
                    Encoding.ASCII.GetBytes(GerarHashSHA256Legado(senha)));

                if (igual)
                {
                    usuario.AtualizarSenhaHash(_hasher.HashPassword(usuario, senha));
                    await _usuarioRepository.UpdateAsync(usuario);
                }
                return igual;
            }

            var resultado = _hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
            if (resultado == PasswordVerificationResult.Failed)
                return false;

            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.AtualizarSenhaHash(_hasher.HashPassword(usuario, senha));
                await _usuarioRepository.UpdateAsync(usuario);
            }
            return true;
        }

        private string GerarTokenJwt(Usuario usuario)
        {
            var chaveSecreta = _configuration["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Chave secreta do JWT não configurada.");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nome),
                    new Claim(ClaimTypes.Email, usuario.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta)),
                    SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        private static bool EhHashLegado(string hash) =>
            hash.Length == 64 && hash.All(Uri.IsHexDigit);

        private static string GerarHashSHA256Legado(string senha) =>
            Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(senha)));
    }
}
