using Microsoft.EntityFrameworkCore;
using TiendaEntityFramework.Models;

namespace TiendaEntityFramework.Data
{
    public class TiendaContext : DbContext
    {
        // El constructor recibe la configuración (cadena de conexión, dialecto SQL) 
        // desde Program.cs a través de inyección de dependencias 
        public TiendaContext(DbContextOptions<TiendaContext> options)
            : base(options)
        {
        }

        // Cada DbSet<T> representa una tabla en la base de datos 

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Orden> Ordenes { get; set; }
        public DbSet<DetalleOrden> DetallesOrden { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)

        {

            // ── Configurar tipos de columna para decimales ──────────────── 
            // EF no sabe qué precisión decimal usar; hay que indicársela 

            modelBuilder.Entity<Producto>()
                .Property(p => p.Precio)
                .HasColumnType("decimal(18,2)");



            modelBuilder.Entity<Orden>()
                .Property(o => o.Total)
                .HasColumnType("decimal(18,2)");



            modelBuilder.Entity<DetalleOrden>()
                .Property(d => d.PrecioUnitario)
                .HasColumnType("decimal(18,2)");



            // ── Relación Categoria → Producto (1-N) ─────────────────────── 
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)           // un producto tiene UNA categoría 
                .WithMany(c => c.Productos)         // una categoría tiene MUCHOS productos 
                .HasForeignKey(p => p.CategoriaId)  // la FK es CategoriaId 
                .OnDelete(DeleteBehavior.Restrict);  // no borrar categorías con productos 



            // ── Relación Orden → DetalleOrden (1-N) ────────────────────── 
            modelBuilder.Entity<DetalleOrden>()
                .HasOne(d => d.Orden)
                .WithMany(o => o.Detalles)
                .HasForeignKey(d => d.OrdenId)
                .OnDelete(DeleteBehavior.Cascade);  // borrar orden borra sus detalles 



            // ── Relación Producto → DetalleOrden (1-N) ─────────────────── 
            modelBuilder.Entity<DetalleOrden>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesOrden)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);  // no borrar productos con detalles 



            // ── Datos iniciales (seed) ──────────────────────────────────── 
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nombre = "Electrónica", Descripcion = "Gadgets y dispositivos" },
                new Categoria { Id = 2, Nombre = "Ropa", Descripcion = "Indumentaria" },
                new Categoria { Id = 3, Nombre = "Alimentos", Descripcion = "Comestibles" }
            );

        }
    }
}
