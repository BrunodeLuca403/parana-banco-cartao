# Paraná Banco — Cartão de Crédito

Microsserviço que consome o evento `PropostaGerada` e emite os cartões de crédito
conforme a faixa aprovada. É o fim do fluxo de concessão de crédito.

## Arquitetura

```
Cartao.API            → Controllers, Program.cs, configuração
Cartao.Application    → Services (emissão/consulta), DTOs, Common
Cartao.Core           → Entidade Cartao, enum de bandeira, contrato do evento, repositório
Cartao.Infrastructure → EF Core (SQL Server), repositório, mensageria (consumer)
Cartao.Teste          → Testes de unidade (xUnit + Moq + Bogus)
```

Os namespaces usam a raiz `ParanaBanco.Cartoes` (plural) para evitar colisão com a
entidade `Cartao` (singular).

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/v1/cartoes/{clienteId}` | Lista os cartões de um cliente |

Os cartões não são criados via REST — são emitidos automaticamente ao consumir o
evento `PropostaGerada`.

## Regra de emissão

A partir do evento `PropostaGerada`, o serviço emite os cartões conforme o resultado:

| Situação do evento | Ação |
|--------------------|------|
| Não aprovada (`Aprovada = false`) | Nenhum cartão |
| Aprovada, 1 cartão | 1 cartão com o limite informado |
| Aprovada, até 2 cartões | 2 cartões, cada um com o limite informado |

Cada cartão recebe um número gerado e o limite informado. O serviço confia no
resultado enviado pela Proposta — não recalcula o score. A quantidade de cartões e o
limite vêm do evento; a bandeira não é regra do desafio (os cartões são emitidos como Visa).

## Mensageria

**Consome** `PropostaGerada`:
- Fila: `cartao.proposta-gerada`, ligada à exchange `proposta.exchange`
  (routing key `proposta.gerada`).

### Resiliência

- **Retry com backoff exponencial**: até 5 tentativas (2s, 4s, 8s, 16s).
- **Dead-letter queue**: mensagens que esgotam as tentativas, ou com payload inválido,
  vão para `cartao.proposta-gerada.dlq` via a dead-letter exchange `cartao.dlx`.
- **Idempotência**: se o cliente já possui cartões (entrega duplicada), o evento é
  tratado como sucesso, sem reemitir.

## Variáveis de ambiente / configuração

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SEU_SERVIDOR;Database=ParanaBancoCartao;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "RabbitMq": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "Exchange": "proposta.exchange",
    "PropostaGeradaRoutingKey": "proposta.gerada",
    "Queue": "cartao.proposta-gerada",
    "DeadLetterExchange": "cartao.dlx",
    "DeadLetterRoutingKey": "cartao.proposta-gerada.dead",
    "DeadLetterQueue": "cartao.proposta-gerada.dlq",
    "MaxRetryAttempts": 5,
    "RetryBaseDelaySeconds": 2
  }
}
```

## Como executar localmente

Pré-requisitos: .NET 10 SDK, SQL Server, RabbitMQ (ver README do serviço de Clientes
para subir o RabbitMQ via Docker).

1. **Aplicar as migrations:**
   ```bash
   dotnet ef database update --project Cartao.Infrastructure --startup-project Cartao.API
   ```

2. **Rodar a API:**
   ```bash
   dotnet run --project Cartao.API
   ```
   Disponível em `http://localhost:5139`. O consumer inicia automaticamente, declara a
   própria fila e o binding na `proposta.exchange`, e passa a escutar.

## Testes

```bash
dotnet test
```

Cobrem a regra de emissão (quantidade de cartões por cenário, idempotência, proposta
negada) e a entidade `Cartao` (validações e geração do número).

## Decisões técnicas

- **O consumidor é dono da fila e do binding**: o serviço declara `cartao.proposta-gerada`
  e a liga à `proposta.exchange` no startup (idempotente). O produtor (Proposta) não
  conhece esta fila — publica apenas na exchange.
- **Confia no resultado da Proposta**: a faixa de score é responsabilidade da Proposta;
  o Cartão apenas executa a emissão com os dados recebidos (`Aprovada`, `QuantidadeCartoes`,
  `LimitePorCartao`). Separação clara de responsabilidades.
- **Emissão em lote numa transação**: os cartões de um cliente são persistidos juntos,
  garantindo consistência (ou todos, ou nenhum).
- **Entidade rica**: `Cartao.Emitir` é um factory method que valida as invariantes e
  gera o número, impedindo a criação de um cartão inválido.
```
