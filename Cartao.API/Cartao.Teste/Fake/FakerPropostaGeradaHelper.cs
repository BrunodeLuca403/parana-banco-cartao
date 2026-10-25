using Bogus;
using ParanaBanco.Cartoes.Core.Events;

namespace ParanaBanco.Cartoes.Teste.Helpers
{
    public static class FakerPropostaGeradaHelper
    {
        private static readonly Faker _faker = new("pt_BR");

        // Proposta aprovada com N cartões e um limite por cartão.
        public static PropostaGeradaEvent Aprovada(int quantidadeCartoes, decimal limitePorCartao) =>
            new(
                PropostaId: _faker.Random.Guid(),
                ClienteId: _faker.Random.Guid(),
                Score: _faker.Random.Int(501, 1000),
                Aprovada: true,
                LimitePorCartao: limitePorCartao,
                QuantidadeCartoes: quantidadeCartoes,
                OcorridoEm: DateTime.UtcNow);

        // Proposta negada: sem limite, sem cartões.
        public static PropostaGeradaEvent Negada() =>
            new(
                PropostaId: _faker.Random.Guid(),
                ClienteId: _faker.Random.Guid(),
                Score: _faker.Random.Int(0, 100),
                Aprovada: false,
                LimitePorCartao: 0m,
                QuantidadeCartoes: 0,
                OcorridoEm: DateTime.UtcNow);
    }
}
