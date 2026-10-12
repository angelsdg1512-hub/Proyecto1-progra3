using Microsoft.EntityFrameworkCore;
using ManagementSystem.Domain.Entities;
using ManagementSystem.DataAccess.Interfaces;
namespace ManagementSystem.DataAccess.EntityFramework;

// Consulta EF delimitada; ADO.NET sigue siendo el acceso principal para escritura.
public class ProductoConsultaEF : IProductoConsultaEF
{
    public List<Producto> GetByCategoria(int categoriaId, bool soloActivos = true)
    {
        using var context = new ManagementDbContext();
        return context.Productos.AsNoTracking()
            .Where(p => p.CategoriaId == categoriaId && (!soloActivos || p.Activo))
            .OrderBy(p => p.Nombre).ToList();
    }
    public List<Producto> GetStockBajo()
    {
        using var context = new ManagementDbContext();
        return context.Productos.AsNoTracking()
            .Where(p => p.Activo && p.StockActual <= p.StockMinimo)
            .OrderBy(p => p.StockActual).ThenBy(p => p.Nombre).ToList();
    }
}
