using ParanaBanco.Cartoes.Core.Entities;
using ParanaBanco.Cartoes.Core.Enums;

namespace ParanaBanco.Cartoes.Application.DTOs
{
    public class CartaoResponse
    {
        public Guid Id { get; set; }
        public Guid IdCliente { get; set; }
        public string Numero { get; set; } = string.Empty;
        public decimal Limite { get; set; }
        public BandeiraCartao Bandeira { get; set; }
        public DateTime CreatAt { get; set; }

        public static CartaoResponse FromEntity(Cartao cartao) => new()
        {
            Id = cartao.Id,
            IdCliente = cartao.IdCliente,
            Numero = cartao.Numero,
            Limite = cartao.Limite,
            Bandeira = cartao.Bandeira,
            CreatAt = cartao.CreatAt
        };
    }
}
