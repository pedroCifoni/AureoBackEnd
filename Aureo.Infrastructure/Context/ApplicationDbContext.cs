using Aureo.Domain.Entities;
using Microsoft.EntityFrameworkCore;




namespace Aureo.Infrastructure.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Transacao> Transacoes { get; set; }
        public DbSet<Categoria> Categorias { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
           

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.Property(e => e.SenhaHash).IsRequired();
                entity.Property(e => e.CriadoEm).IsRequired();

                entity.HasIndex(e => e.Nome).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable("Categorias");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Nome).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Tipo).IsRequired();
                entity.Property(e => e.Cor).IsRequired().HasMaxLength(7);
                entity.Property(e => e.Icone).HasMaxLength(50);

                entity.HasOne<Usuario>()
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UsuarioId, e.Nome, e.Tipo }).IsUnique();
            });

            modelBuilder.Entity<Transacao>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Descricao)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Valor)
                    .HasColumnType("numeric(18,2)")
                    .IsRequired();

                entity.Property(e => e.Tipo).IsRequired();
                entity.Property(e => e.Data).IsRequired();

                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Restrict: categoria em uso não pode sumir (o service devolve 409 antes de chegar aqui).
                entity.HasOne(e => e.Categoria)
                    .WithMany()
                    .HasForeignKey(e => e.CategoriaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.UsuarioId, e.Data });
                entity.HasIndex(e => e.CategoriaId);
            });
        }
    }
}
