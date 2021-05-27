using Vaivoa.CartoesController.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Vaivoa.CartoesController.Persistencia
{
    public class CartaoContext : DbContext
    {
        public DbSet<Cartao> Cartoes { get; set; }

        public CartaoContext(DbContextOptions<CartaoContext> options) 
            : base(options)
        {
            //irá criar o banco e a estrutura de tabelas necessárias
            this.Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration<Cartao>(new CartaoConfiguration());
        }
    }
}
