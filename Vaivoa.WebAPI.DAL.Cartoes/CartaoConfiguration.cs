using Vaivoa.CartoesController.Modelos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Vaivoa.CartoesController.Persistencia
{
    internal class CartaoConfiguration : IEntityTypeConfiguration<Cartao>
    {
        public void Configure(EntityTypeBuilder<Cartao> builder)
        {
            builder
               .Property(l => l.Email)
               .HasColumnType("nvarchar(200)")
               .IsRequired();

            builder
                .Property(l => l.Titular)
                .HasColumnType("nvarchar(200)")
                .IsRequired();

            builder
                .Property(l => l.Numero)
                .HasColumnType("nvarchar(20)");

            builder
                .Property(l => l.CodSeguranca)
                .HasColumnType("nvarchar(3)");

            builder
                .Property(l => l.MesValido)
                .HasColumnType("nvarchar(2)");

            builder
                .Property(l => l.AnoValido)
                .HasColumnType("nvarchar(4)");

        }
    }
}