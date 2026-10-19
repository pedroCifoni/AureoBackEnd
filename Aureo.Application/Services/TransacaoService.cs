using Aureo.Application.DTOs.Transacao;
using Aureo.Application.Interfaces;
using Aureo.Domain.Common;
using Aureo.Domain.Entities;
using Aureo.Domain.Filtros;
using Aureo.Domain.Interfaces;

namespace Aureo.Application.Services
{
    public class TransacaoService : ITransacaoService
    {
        private readonly ITransacaoRepository _repository;

        public TransacaoService(ITransacaoRepository repository)
        {
            _repository = repository;
        }

        public async Task<TransacaoResponseDto> CriarAsync(Guid usuarioId, CriarTransacaoDto dto)
        {
            var transacao = Transacao.Criar(
                dto.Descricao,
                dto.Categoria,
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
                query.Categoria,
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

            transacao.Atualizar(dto.Descricao, dto.Categoria, dto.Valor, dto.Tipo, dto.Data);
            await _repository.UpdateAsync(transacao);

            return ToDto(transacao);
        }

        public async Task DeletarAsync(Guid id, Guid usuarioId)
        {
            var transacao = await _repository.GetByIdAsync(id, usuarioId)
                ?? throw new KeyNotFoundException("Transação não encontrada.");

            await _repository.DeleteAsync(transacao);
        }

        private static TransacaoResponseDto ToDto(Transacao t) => new(
            t.Id,
            t.Descricao,
            t.Categoria,
            t.Valor,
            t.Tipo,
            t.Data
        );
    }
}
