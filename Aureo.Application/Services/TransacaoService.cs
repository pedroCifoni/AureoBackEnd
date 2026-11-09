using Aureo.Application.DTOs.Transacao;
using Aureo.Application.Interfaces;
using Aureo.Domain.Common;
using Aureo.Domain.Entities;
using Aureo.Domain.Exceptions;
using Aureo.Domain.Filtros;
using Aureo.Domain.Interfaces;

namespace Aureo.Application.Services
{
    public class TransacaoService : ITransacaoService
    {
        private readonly ITransacaoRepository _repository;
        private readonly ICategoriaRepository _categoriaRepository;

        public TransacaoService(ITransacaoRepository repository, ICategoriaRepository categoriaRepository)
        {
            _repository = repository;
            _categoriaRepository = categoriaRepository;
        }

        public async Task<TransacaoResponseDto> CriarAsync(Guid usuarioId, CriarTransacaoDto dto)
        {
            var categoria = await ObterCategoriaAsync(dto.CategoriaId, usuarioId);

            var transacao = Transacao.Criar(
                dto.Descricao,
                categoria,
                dto.Valor,
                dto.Tipo,
                dto.Data,
                usuarioId
            );

            await _repository.AddAsync(transacao);

            return ToDto(transacao);
        }

        public async Task<PagedResult<TransacaoResponseDto>> ListarAsync(Guid usuarioId, ListarTransacoesQuery query)
        {
            var filtro = new TransacaoFiltro(
                query.DataInicio is { } inicio ? DataUtc.Normalizar(inicio) : null,
                query.DataFim is { } fim ? DataUtc.Normalizar(fim) : null,
                query.Tipo,
                query.CategoriaId,
                query.Busca,
                query.OrdenarPor,
                query.Decrescente,
                query.Pagina,
                query.Tamanho);

            var pagina = await _repository.ListarAsync(usuarioId, filtro);
            return pagina.Map(ToDto);
        }

        public async Task<TransacaoResponseDto> ObterPorIdAsync(Guid id, Guid usuarioId)
        {
            var transacao = await _repository.GetByIdAsync(id, usuarioId)
                ?? throw new KeyNotFoundException("Transação não encontrada.");

            return ToDto(transacao);
        }

        public async Task<TransacaoResponseDto> AtualizarAsync(Guid id, Guid usuarioId, AtualizarTransacaoDto dto)
        {
            var transacao = await _repository.GetByIdAsync(id, usuarioId)
                ?? throw new KeyNotFoundException("Transação não encontrada.");

            var categoria = await ObterCategoriaAsync(dto.CategoriaId, usuarioId);

            transacao.Atualizar(dto.Descricao, categoria, dto.Valor, dto.Tipo, dto.Data);
            await _repository.UpdateAsync(transacao);

            return ToDto(transacao);
        }

        public async Task DeletarAsync(Guid id, Guid usuarioId)
        {
            var transacao = await _repository.GetByIdAsync(id, usuarioId)
                ?? throw new KeyNotFoundException("Transação não encontrada.");

            await _repository.DeleteAsync(transacao);
        }

        // Categoria de outro usuário se comporta como inexistente (nunca vaza que existe).
        private async Task<Categoria> ObterCategoriaAsync(Guid categoriaId, Guid usuarioId)
        {
            return await _categoriaRepository.GetByIdAsync(categoriaId, usuarioId)
                ?? throw new DominioException("Categoria inválida.");
        }

        private static TransacaoResponseDto ToDto(Transacao t) => new(
            t.Id,
            t.Descricao,
            t.CategoriaId,
            t.Categoria.Nome,
            t.Valor,
            t.Tipo,
            t.Data
        );
    }
}
