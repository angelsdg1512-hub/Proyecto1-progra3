using ManagementSystem.Domain.Entities;
namespace ManagementSystem.Business.Reportes;

// Complemento: estas consultas corren sobre List<T> ya obtenido por ADO.NET.
// Copiar a la capa Negocio cuando exista; no forma parte de DataAccess.
public static class ReportesLINQ
{
    public static List<Producto> StockBajo(List<Producto> productos) =>
        productos.Where(p => p.Activo && p.StockActual <= p.StockMinimo).ToList();
    public static List<Producto> OrdenarPorPrecioYNombre(List<Producto> productos) =>
        productos.OrderBy(p => p.PrecioVenta).ThenBy(p => p.Nombre).ToList();
    public static List<Producto> Buscar(List<Producto> productos, string texto) =>
        productos.Where(p => p.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase)
            || p.Descripcion.Contains(texto, StringComparison.OrdinalIgnoreCase)).ToList();
    public static List<TotalVentasCliente> TotalPorCliente(List<Venta> ventas) =>
        ventas.Where(v => v.Estado == "Procesada").GroupBy(v => v.ClienteId)
            .Select(g => new TotalVentasCliente(g.Key, g.Count(), g.Sum(v => v.Total))).ToList();
    public static EstadisticasProductos Estadisticas(List<Producto> productos)
    {
        var activos = productos.Where(p => p.Activo).ToList();
        if (activos.Count == 0) return new(0, 0, 0, 0, 0, 0);
        return new(activos.Count, activos.Min(p => p.PrecioVenta), activos.Max(p => p.PrecioVenta),
            activos.Average(p => p.PrecioVenta), activos.Sum(p => (long)p.StockActual),
            activos.Sum(p => p.StockActual * p.PrecioCompra));
    }
}
public record TotalVentasCliente(int ClienteId, int CantidadVentas, decimal Total);
public record EstadisticasProductos(int Cantidad, decimal PrecioMinimo, decimal PrecioMaximo,
    decimal PrecioPromedio, long Unidades, decimal ValorInventarioCompra);
