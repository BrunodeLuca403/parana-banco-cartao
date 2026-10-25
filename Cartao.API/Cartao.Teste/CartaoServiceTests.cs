using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ParanaBanco.Cartoes.Application.Services.Implementations;
using ParanaBanco.Cartoes.Core.Entities;
using ParanaBanco.Cartoes.Core.Repositories;
using ParanaBanco.Cartoes.Teste.Helpers;

namespace ParanaBanco.Cartoes.Teste
{
    public class CartaoServiceTests
    {
        private readonly Mock<ICartaoRepository> _repositoryMock = new();
        private readonly CartaoService _sut;

        public CartaoServiceTests()
        {
            _sut = new CartaoService(_repositoryMock.Object, NullLogger<CartaoService>.Instance);
        }

        [Fact]
        public async Task ProcessarPropostaGerada_QuandoAprovadaComDoisCartoes_DeveEmitirDois()
        {
            var evento = FakerPropostaGeradaHelper.Aprovada(quantidadeCartoes: 2, limitePorCartao: 5000m);
            _repositoryMock.Setup(r => r.ExistePorClienteAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            await _sut.ProcessarPropostaGeradaAsync(evento);

            _repositoryMock.Verify(r => r.AdicionarAsync(
                It.Is<IEnumerable<Cartao>>(cs => cs.Count() == 2 && cs.All(c => c.Limite == 5000m))),
                Times.Once);
        }

        [Fact]
        public async Task ProcessarPropostaGerada_QuandoAprovadaComUmCartao_DeveEmitirUm()
        {
            var evento = FakerPropostaGeradaHelper.Aprovada(quantidadeCartoes: 1, limitePorCartao: 1000m);
            _repositoryMock.Setup(r => r.ExistePorClienteAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            await _sut.ProcessarPropostaGeradaAsync(evento);

            _repositoryMock.Verify(r => r.AdicionarAsync(
                It.Is<IEnumerable<Cartao>>(cs => cs.Count() == 1 && cs.All(c => c.Limite == 1000m))),
                Times.Once);
        }

        [Fact]
        public async Task ProcessarPropostaGerada_QuandoNaoAprovada_NaoDeveEmitir()
        {
            var evento = FakerPropostaGeradaHelper.Negada();

            await _sut.ProcessarPropostaGeradaAsync(evento);

            _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<IEnumerable<Cartao>>()), Times.Never);
        }

        [Fact]
        public async Task ProcessarPropostaGerada_QuandoClienteJaTemCartoes_NaoDeveEmitirNovamente()
        {
            // Idempotência: evento duplicado não reemite.
            var evento = FakerPropostaGeradaHelper.Aprovada(quantidadeCartoes: 2, limitePorCartao: 5000m);
            _repositoryMock.Setup(r => r.ExistePorClienteAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            await _sut.ProcessarPropostaGeradaAsync(evento);

            _repositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<IEnumerable<Cartao>>()), Times.Never);
        }
    }
}
