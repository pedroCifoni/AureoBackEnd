using Aureo.Application.DTOs.Transacao;
using Aureo.Application.Interfaces;
using Aureo.Domain.Entities;
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

        public async Task<IEnumerable<TransacaoResponseDto>> ObterTodasAsync(Guid usuarioId)
        {
            var transacoes = await _repository.GetAllByUsuarioAsync(usuarioId);
            return transacoes.Select(ToDto);
        }

        public async Task<TransacaoResponseDto> ObterPorIdAsync(Guid id, Guid usuarioId)
        {
            var transacao = await _repository.GetByIdAsync(id, usuarioId)
                ?? throw new KeyNotFoundException("Transação não encontrada.");

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