using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParanaBanco.Cartoes.Core.Entities;

namespace ParanaBanco.Cartoes.Infrastructure.Persistence.Configuration
{
    public class CartaoEntityConfiguration : IEntityTypeConfiguration<Cartao>
    {
        public void Configure(EntityTypeBuilder<Cartao> builder)
        {
            builder.ToTable("Cartoes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.IdCliente)
                .IsRequired();

            builder.Property(x => x.Numero)
                .HasColumnType("char(16)")
                .IsRequired();

            builder.Property(x => x.Limite)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.Bandeira)
                .IsRequired();

            builder.Property(x => x.CriadoEm)
                .HasColumnType("datetime")
                .IsRequired();

            builder.HasIndex(x => x.IdCliente);
        }
    }
}
