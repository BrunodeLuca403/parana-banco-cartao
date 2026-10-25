using Microsoft.EntityFrameworkCore;
using ParanaBanco.Cartoes.Core.Entities;
using ParanaBanco.Cartoes.Core.Repositories;
using ParanaBanco.Cartoes.Infrastructure.Persistence;

namespace ParanaBanco.Cartoes.Infrastructure.Repository
{
    public class CartaoRepository : ICartaoRepository
    {
        private readonly CartaoDbContext _db;

        public CartaoRepository(CartaoDbContext db)
        {
            _db = db;
        }

        public async Task AdicionarAsync(IEnumerable<Cartao> cartoes)
        {
            await _db.Cartoes.AddRangeAsync(cartoes);
            await _db.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<Cartao>> ListarPorClienteAsync(Guid clienteId)
        {
            return await _db.Cartoes
                .Where(c => c.IdCliente == clienteId)
                .ToListAsync();
        }

        public async Task<bool> ExistePorClienteAsync(Guid clienteId)
        {
            return await _db.Cartoes.AnyAsync(c => c.IdCliente == clienteId);
        }
    }
}
