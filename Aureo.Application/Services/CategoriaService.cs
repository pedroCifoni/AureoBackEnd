using Aureo.Application.DTOs.Categoria;
using Aureo.Application.Interfaces;
using Aureo.Domain.Entities;
using Aureo.Domain.Exceptions;
using Aureo.Domain.Interfaces;

namespace Aureo.Application.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _repository;

        public CategoriaService(ICategoriaRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyList<CategoriaResponseDto>> ListarAsync(
            Guid usuarioId, TipoTransacao? tipo, CancellationToken ct = default)
        {
            var categorias = await _repository.ListarAsync(usuarioId, tipo, ct);
            return categorias.Select(ToDto).ToList();
        }

        public async Task<CategoriaResponseDto> ObterPorIdAsync(
            Guid id, Guid usuarioId, CancellationToken ct = default)
        {
            var categoria = await _repository.GetByIdAsync(id, usuarioId, ct)
                ?? throw new KeyNotFoundException("Categoria não encontrada.");

            return ToDto(categoria);
        }

        public async Task<CategoriaResponseDto> CriarAsync(
            Guid usuarioId, CriarCategoriaDto dto, CancellationToken ct = default)
        {
            var categoria = Categoria.Criar(dto.Nome, dto.Tipo, dto.Cor, dto.Icone, usuarioId);

            if (await _repository.ExisteNomeAsync(usuarioId, categoria.Nome, categoria.Tipo, null, ct))
                throw new ConflitoException("Já existe uma categoria com este nome.");

            await _repository.AddAsync(categoria, ct);
            return ToDto(categoria);
        }

        public async Task<CategoriaResponseDto> AtualizarAsync(
            Guid id, Guid usuarioId, AtualizarCategoriaDto dto, CancellationToken ct = default)
        {
            var categoria = await _repository.GetByIdAsync(id, usuarioId, ct)
                ?? throw new KeyNotFoundException("Categoria não encontrada.");

            categoria.Atualizar(dto.Nome, dto.Cor, dto.Icone);

            if (await _repository.ExisteNomeAsync(usuarioId, categoria.Nome, categoria.Tipo, categoria.Id, ct))
                throw new ConflitoException("Já existe uma categoria com este nome.");

            await _repository.UpdateAsync(categoria, ct);
            return ToDto(categoria);
        }

        public async Task DeletarAsync(Guid id, Guid usuarioId, CancellationToken ct = default)
        {
            var categoria = await _repository.GetByIdAsync(id, usuarioId, ct)
                ?? throw new KeyNotFoundException("Categoria não encontrada.");

            if (await _repository.EmUsoAsync(categoria.Id, ct))
                throw new ConflitoException("Categoria em uso por transações e não pode ser excluída.");

            await _repository.DeleteAsync(categoria, ct);
        }

        private static CategoriaResponseDto ToDto(Categoria c) => new(c.Id, c.Nome, c.Tipo, c.Cor, c.Icone);
    }
}
