using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParanaBanco.Cartoes.Application.Services.Interface;
using ParanaBanco.Cartoes.Core.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ParanaBanco.Cartoes.Infrastructure.Messaging
{
    public class PropostaGeradaConsumer : BackgroundService
    {
        private readonly IRabbitMqConnection _connection;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<PropostaGeradaConsumer> _logger;

        private IChannel? _channel;

        public PropostaGeradaConsumer(
            IRabbitMqConnection connection,
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqSettings> options,
            ILogger<PropostaGeradaConsumer> logger)
        {
            _connection = connection;
            _scopeFactory = scopeFactory;
            _settings = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _channel = await _connection.CreateChannelAsync(stoppingToken);

            await DeclararTopologiaAsync(stoppingToken);

            await _channel.BasicQosAsync(0, 1, false, stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += OnMessageReceivedAsync;

            await _channel.BasicConsumeAsync(
                queue: _settings.Queue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            _logger.LogInformation("Consumer iniciado. Escutando a fila {Queue}", _settings.Queue);

            await Task.Delay(Timeout.Infinite, stoppingToken)
                .ContinueWith(_ => { }, TaskScheduler.Default);
        }

        private async Task DeclararTopologiaAsync(CancellationToken ct)
        {
            await _channel!.ExchangeDeclareAsync(
                _settings.Exchange, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: ct);

            await _channel.ExchangeDeclareAsync(
                _settings.DeadLetterExchange, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: ct);

            await _channel.QueueDeclareAsync(
                _settings.DeadLetterQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
            await _channel.QueueBindAsync(
                _settings.DeadLetterQueue, _settings.DeadLetterExchange, _settings.DeadLetterRoutingKey, cancellationToken: ct);

            var args = new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = _settings.DeadLetterExchange,
                ["x-dead-letter-routing-key"] = _settings.DeadLetterRoutingKey
            };
            await _channel.QueueDeclareAsync(
                _settings.Queue, durable: true, exclusive: false, autoDelete: false, arguments: args, cancellationToken: ct);

            await _channel.QueueBindAsync(
                _settings.Queue, _settings.Exchange, _settings.PropostaGeradaRoutingKey, cancellationToken: ct);

            _logger.LogInformation(
                "Topologia declarada: fila {Queue} ligada a {Exchange} ({RoutingKey}), DLQ {Dlq}",
                _settings.Queue, _settings.Exchange, _settings.PropostaGeradaRoutingKey, _settings.DeadLetterQueue);
        }

        private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());

            PropostaGeradaEvent? evento;
            try
            {
                evento = JsonSerializer.Deserialize<PropostaGeradaEvent>(json);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON inválido. Enviando para a DLQ. Payload: {Payload}", json);
                await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            if (evento is null)
            {
                _logger.LogError("Mensagem desserializada como nula. Enviando para a DLQ.");
                await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            var sucesso = await ProcessarComRetryAsync(evento);

            if (sucesso)
            {
                await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            else
            {
                _logger.LogWarning(
                    "Falha após {Max} tentativas. Enviando para a DLQ. ClienteId={ClienteId}",
                    _settings.MaxRetryAttempts, evento.ClienteId);
                await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            }
        }

        private async Task<bool> ProcessarComRetryAsync(PropostaGeradaEvent evento)
        {
            for (var tentativa = 1; tentativa <= _settings.MaxRetryAttempts; tentativa++)
            {
                try
                {
                    await ProcessarEventoAsync(evento);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Tentativa {Tentativa}/{Max} falhou para ClienteId={ClienteId}",
                        tentativa, _settings.MaxRetryAttempts, evento.ClienteId);

                    if (tentativa == _settings.MaxRetryAttempts)
                        return false;

                    var delay = TimeSpan.FromSeconds(_settings.RetryBaseDelaySeconds * Math.Pow(2, tentativa - 1));
                    await Task.Delay(delay);
                }
            }

            return false;
        }

        private async Task ProcessarEventoAsync(PropostaGeradaEvent evento)
        {
            using var scope = _scopeFactory.CreateScope();
            var cartaoService = scope.ServiceProvider.GetRequiredService<ICartaoService>();
            await cartaoService.ProcessarPropostaGeradaAsync(evento);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null)
            {
                await _channel.CloseAsync(cancellationToken);
                await _channel.DisposeAsync();
            }
            await base.StopAsync(cancellationToken);
        }
    }
}
