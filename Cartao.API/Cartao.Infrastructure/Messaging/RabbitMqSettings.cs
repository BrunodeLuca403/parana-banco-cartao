namespace ParanaBanco.Cartoes.Infrastructure.Messaging
{
    public class RabbitMqSettings
    {
        public const string SectionName = "RabbitMq";

        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public string Exchange { get; set; } = "proposta.exchange";
        public string PropostaGeradaRoutingKey { get; set; } = "proposta.gerada";
        public string Queue { get; set; } = "cartao.proposta-gerada";
        public string DeadLetterExchange { get; set; } = "cartao.dlx";
        public string DeadLetterRoutingKey { get; set; } = "cartao.proposta-gerada.dead";
        public string DeadLetterQueue { get; set; } = "cartao.proposta-gerada.dlq";
        public int MaxRetryAttempts { get; set; } = 5;
        public int RetryBaseDelaySeconds { get; set; } = 2;
    }
}
