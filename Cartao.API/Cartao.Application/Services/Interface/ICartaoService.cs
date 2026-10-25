using ParanaBanco.Cartoes.Application.Common;
using ParanaBanco.Cartoes.Application.DTOs;
using ParanaBanco.Cartoes.Core.Events;

namespace ParanaBanco.Cartoes.Application.Services.Interface
{
    public interface ICartaoService
    {
        Task<ResultViewModel<List<CartaoResponse>>> ListarPorClienteAsync(Guid clienteId);
        Task ProcessarPropostaGeradaAsync(PropostaGeradaEvent evento, CancellationToken cancellationToken = default);
    }
}
