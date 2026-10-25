using ParanaBanco.Cartoes.Core.Entities;
using ParanaBanco.Cartoes.Core.Enums;

namespace ParanaBanco.Cartoes.Teste
{
    public class CartaoEntityTests
    {
        [Fact]
        public void Emitir_QuandoDadosValidos_DeveGerarNumeroDe16Digitos()
        {
            var cartao = Cartao.Emitir(Guid.NewGuid(), 5000m, BandeiraCartao.Visa);

            Assert.Equal(16, cartao.Numero.Length);
            Assert.All(cartao.Numero, c => Assert.True(char.IsDigit(c)));
            Assert.Equal(5000m, cartao.Limite);
        }

        [Fact]
        public void Emitir_QuandoIdClienteVazio_DeveLancarArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                Cartao.Emitir(Guid.Empty, 5000m, BandeiraCartao.Visa));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void Emitir_QuandoLimiteInvalido_DeveLancarArgumentOutOfRange(decimal limite)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Cartao.Emitir(Guid.NewGuid(), limite, BandeiraCartao.Mastercard));
        }
    }
}
