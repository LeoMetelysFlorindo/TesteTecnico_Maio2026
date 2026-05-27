using Microsoft.EntityFrameworkCore;
using PedioApi.models;


namespace PedioApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets for each entity
        public DbSet<Pedido> Pedidos => Set<Pedido>();
        public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.ToTable("Pedido");

                entity.HasIndex(p => p.ClienteNome);

                entity.Property(p => p.ValorTotal)
                    .HasPrecision(18, 2);

                entity.HasMany(p => p.ItensPedido)
                    .WithOne(i => i.Pedido)
                    .HasForeignKey(i => i.PedidoId)
                    .OnDelete(DeleteBehavior.Cascade);

            });

            modelBuilder.Entity<Pedido>()
                 .Property(p => p.Status)
                 .HasConversion<string>()
                 .HasMaxLength(10);




            modelBuilder.Entity<ItemPedido>(entity =>
            {
                entity.ToTable("ItemPedido");

                entity.Property(i => i.NomeProduto)
                    .HasMaxLength(100);

                entity.Property(i => i.ValorUnitario)
                    .HasPrecision(18, 2);
            });

         
        }
    }
}
