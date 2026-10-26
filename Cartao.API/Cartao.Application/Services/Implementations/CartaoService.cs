using Microsoft.Extensions.Logging;
using ParanaBanco.Cartoes.Application.Common;
using ParanaBanco.Cartoes.Application.DTOs;
using ParanaBanco.Cartoes.Application.Services.Interface;
using ParanaBanco.Cartoes.Core.Entities;
using ParanaBanco.Cartoes.Core.Enums;
using ParanaBanco.Cartoes.Core.Events;
using ParanaBanco.Cartoes.Core.Repositories;

namespace ParanaBanco.Cartoes.Application.Services.Implementations
{
    public class CartaoService : ICartaoService
    {
        private readonly ICartaoRepository _cartaoRepository;
        private readonly ILogger<CartaoService> _logger;

        public CartaoService(ICartaoRepository cartaoRepository, ILogger<CartaoService> logger)
        {
            _cartaoRepository = cartaoRepository;
            _logger = logger;
        }

        public async Task<ResultViewModel<List<CartaoResponse>>> ListarPorClienteAsync(Guid clienteId)
        {
            var cartoes = await _cartaoRepository.ListarPorClienteAsync(clienteId);

            if (cartoes.Count == 0)
            {
                return ResultViewModel<List<CartaoResponse>>.Failure(
                    Error.NotFound(
                        "Cartao.Application.Services.ListarPorCliente",
                        $"Nenhum cartão encontrado para o cliente {clienteId}."));
            }

            var response = cartoes.Select(CartaoResponse.FromEntity).ToList();
            return ResultViewModel<List<CartaoResponse>>.Success(response);
        }

        public async Task ProcessarPropostaGeradaAsync(PropostaGeradaEvent evento, CancellationToken cancellationToken = default)
        {
            if (!evento.Aprovada || evento.QuantidadeCartoes <= 0)
            {
                _logger.LogInformation(
                    "Proposta não aprovada para ClienteId={ClienteId}. Nenhum cartão emitido.", evento.ClienteId);
                return;
            }

            if (await _cartaoRepository.ExistePorClienteAsync(evento.ClienteId))
            {
                _logger.LogInformation(
                    "Cliente {ClienteId} já possui cartões (evento duplicado ignorado).", evento.ClienteId);
                return;
            }

            var cartoes = new List<Cartao>();
            for (var i = 0; i < evento.QuantidadeCartoes; i++)
            {
                cartoes.Add(Cartao.Emitir(evento.ClienteId, evento.LimitePorCartao, BandeiraCartao.Visa));
            }

            await _cartaoRepository.AdicionarAsync(cartoes);

            _logger.LogInformation(
                "{Qtd} cartão(ões) emitido(s) para ClienteId={ClienteId}.", cartoes.Count, evento.ClienteId);
        }
    }
}
