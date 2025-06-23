using Microsoft.EntityFrameworkCore;
using CineAtom.WebApi.Models;

namespace CineAtom.WebApi.Data
{
    public class CineAtomDbContext : DbContext
    {
        public CineAtomDbContext(DbContextOptions<CineAtomDbContext> options) : base(options) { }

        public DbSet<Articulo> Articulos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tabla Categoria
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable("CineAtom_Categoria");

                entity.HasKey(e => e.CategoriaId);

                entity.Property(e => e.CategoriaId)
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.Nombre)
                      .HasMaxLength(50)
                      .IsRequired();
            });

            // Tabla Articulo con FK a Categoria
            modelBuilder.Entity<Articulo>(entity =>
            {
                entity.ToTable("CineAtom_Articulo");

                entity.HasKey(e => e.ArticuloId);

                entity.Property(e => e.ArticuloId)
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.Nombre)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.Descripcion)
                      .HasMaxLength(255);

                entity.Property(e => e.Cantidad)
                      .IsRequired()
                      .HasDefaultValue(0);

                entity.Property(e => e.Precio)
                      .HasColumnType("decimal(10,2)")
                      .IsRequired();

                
                entity.HasOne(e => e.Categoria)
                      .WithMany()  
                      .HasForeignKey(e => e.CategoriaId)
                      .OnDelete(DeleteBehavior.Restrict); 
            });
        }
    }
}
