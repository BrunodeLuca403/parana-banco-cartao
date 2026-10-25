namespace ParanaBanco.Cartoes.Core.Events
{
    public record PropostaGeradaEvent(
        Guid PropostaId,
        Guid ClienteId,
        int Score,
        bool Aprovada,
        decimal LimitePorCartao,
        int QuantidadeCartoes,
        DateTime OcorridoEm);
}
