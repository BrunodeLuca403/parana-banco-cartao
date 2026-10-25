using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ParanaBanco.Cartoes.Core.Entities;

namespace ParanaBanco.Cartoes.Infrastructure.Persistence
{
    public class CartaoDbContext : DbContext
    {
        public CartaoDbContext(DbContextOptions<CartaoDbContext> options) : base(options)
        {
        }

        public DbSet<Cartao> Cartoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
