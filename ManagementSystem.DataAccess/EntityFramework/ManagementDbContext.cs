using Microsoft.EntityFrameworkCore;
using ManagementSystem.Domain.Entities;
using ManagementSystem.DataAccess.Connection;
namespace ManagementSystem.DataAccess.EntityFramework;
public class ManagementDbContext : DbContext
{
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public ManagementDbContext() { }
    public ManagementDbContext(DbContextOptions<ManagementDbContext> options) : base(options) { }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured) optionsBuilder.UseSqlServer(DatabaseConfig.ConnectionString);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var categoria = modelBuilder.Entity<Categoria>();
        categoria.ToTable("Categorias", "dbo");
        categoria.HasKey(x => x.CategoriaId);
        categoria.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        categoria.Property(x => x.Descripcion).HasMaxLength(500).IsRequired();
        var producto = modelBuilder.Entity<Producto>();
        producto.ToTable("Productos", "dbo");
        producto.HasKey(x => x.ProductoId);
        producto.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        producto.Property(x => x.Descripcion).HasMaxLength(500).IsRequired();
        producto.Property(x => x.PrecioCompra).HasPrecision(18, 2);
        producto.Property(x => x.PrecioVenta).HasPrecision(18, 2);
        producto.HasOne<Categoria>().WithMany().HasForeignKey(x => x.CategoriaId).OnDelete(DeleteBehavior.Restrict);
    }
}
