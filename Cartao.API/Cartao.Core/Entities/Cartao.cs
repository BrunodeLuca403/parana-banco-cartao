using ParanaBanco.Cartoes.Core.Enums;

namespace ParanaBanco.Cartoes.Core.Entities
{
    public class Cartao : BaseEntity
    {
        private Cartao(Guid idCliente, string numero, decimal limite, BandeiraCartao bandeira)
        {
            IdCliente = idCliente;
            Numero = numero;
            Limite = limite;
            Bandeira = bandeira;
        }

        public Guid IdCliente { get; private set; }
        public string Numero { get; private set; }
        public decimal Limite { get; private set; }
        public BandeiraCartao Bandeira { get; private set; }

        public static Cartao Emitir(Guid idCliente, decimal limite, BandeiraCartao bandeira)
        {
            if (idCliente == Guid.Empty)
                throw new ArgumentException("IdCliente é obrigatório.", nameof(idCliente));

            if (limite <= 0)
                throw new ArgumentOutOfRangeException(nameof(limite), limite, "Limite deve ser maior que zero.");

            return new Cartao(idCliente, GerarNumero(), limite, bandeira);
        }

        private static string GerarNumero()
        {
            var random = Random.Shared;
            Span<char> digitos = stackalloc char[16];
            for (var i = 0; i < 16; i++)
                digitos[i] = (char)('0' + random.Next(0, 10));
            return new string(digitos);
        }
    }
}
