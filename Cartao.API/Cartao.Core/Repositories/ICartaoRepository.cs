using ParanaBanco.Cartoes.Core.Entities;

namespace ParanaBanco.Cartoes.Core.Repositories
{
    public interface ICartaoRepository
    {
        Task AdicionarAsync(IEnumerable<Cartao> cartoes);
        Task<IReadOnlyList<Cartao>> ListarPorClienteAsync(Guid clienteId);
        Task<bool> ExistePorClienteAsync(Guid clienteId);
    }
}
