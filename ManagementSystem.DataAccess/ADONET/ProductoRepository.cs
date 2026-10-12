using System.Data;
using Microsoft.Data.SqlClient;
using ManagementSystem.Domain.Entities;
using ManagementSystem.DataAccess.Interfaces;
namespace ManagementSystem.DataAccess.ADONET;

public class ProductoRepository : RepositoryBase<Producto>, IProductoRepository
{
    public ProductoRepository() : base("Productos", "ProductoId", new[] { "Nombre", "Descripcion", "PrecioCompra", "PrecioVenta", "StockActual", "StockMinimo", "CategoriaId", "FechaRegistro", "Activo" }, true) { }
    protected override int Id(Producto entity) => entity.ProductoId;
    protected override Producto Map(SqlDataReader reader) => new()
    {
        ProductoId = reader.GetInt32(reader.GetOrdinal("ProductoId")),
        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
        Descripcion = reader.GetString(reader.GetOrdinal("Descripcion")),
        PrecioCompra = reader.GetDecimal(reader.GetOrdinal("PrecioCompra")),
        PrecioVenta = reader.GetDecimal(reader.GetOrdinal("PrecioVenta")),
        StockActual = reader.GetInt32(reader.GetOrdinal("StockActual")),
        StockMinimo = reader.GetInt32(reader.GetOrdinal("StockMinimo")),
        CategoriaId = reader.GetInt32(reader.GetOrdinal("CategoriaId")),
        FechaRegistro = reader.GetDateTime(reader.GetOrdinal("FechaRegistro")),
        Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
    };
    protected override void Parameters(SqlCommand command, Producto entity)
    {
        SqlExecutor.Add(command, "@Nombre", SqlDbType.NVarChar, entity.Nombre, 150);
        SqlExecutor.Add(command, "@Descripcion", SqlDbType.NVarChar, entity.Descripcion, 500);
        SqlExecutor.Add(command, "@PrecioCompra", SqlDbType.Decimal, entity.PrecioCompra);
        SqlExecutor.Add(command, "@PrecioVenta", SqlDbType.Decimal, entity.PrecioVenta);
        SqlExecutor.Add(command, "@StockActual", SqlDbType.Int, entity.StockActual);
        SqlExecutor.Add(command, "@StockMinimo", SqlDbType.Int, entity.StockMinimo);
        SqlExecutor.Add(command, "@CategoriaId", SqlDbType.Int, entity.CategoriaId);
        SqlExecutor.Add(command, "@FechaRegistro", SqlDbType.DateTime2, entity.FechaRegistro);
        SqlExecutor.Add(command, "@Activo", SqlDbType.Bit, entity.Activo);
    }
    public List<Producto> Search(string term) => Query("WHERE Nombre LIKE @term OR Descripcion LIKE @term", c => SqlExecutor.Add(c, "@term", SqlDbType.NVarChar, "%" + term + "%", -1));
    public bool CambiarEstado(int id, bool activo) => Execute("UPDATE dbo.Productos SET Activo=@activo WHERE ProductoId=@id", c =>
    { SqlExecutor.Add(c, "@activo", SqlDbType.Bit, activo); SqlExecutor.Add(c, "@id", SqlDbType.Int, id); });
    // Sólo primitiva solicitada por el profesor. Para ventas usar VentaRepository:
    // este método NO registra el histórico ni coordina encabezado/detalles.
    public bool UpdateStock(int idProducto, int cantidadADescontar)
    {
        if (cantidadADescontar <= 0) throw new ArgumentOutOfRangeException(nameof(cantidadADescontar));
        return Execute("UPDATE dbo.Productos SET StockActual=StockActual-@cantidad WHERE ProductoId=@id AND Activo=1 AND StockActual>=@cantidad", c =>
        { SqlExecutor.Add(c, "@cantidad", SqlDbType.Int, cantidadADescontar); SqlExecutor.Add(c, "@id", SqlDbType.Int, idProducto); });
    }
    public override bool Update(Producto entity)
    {
        // Editar datos del producto no modifica existencias: hacerlo con RegistrarCambio.
        return Execute("UPDATE dbo.Productos SET Nombre=@Nombre,Descripcion=@Descripcion,PrecioCompra=@PrecioCompra,PrecioVenta=@PrecioVenta,StockMinimo=@StockMinimo,CategoriaId=@CategoriaId,Activo=@Activo WHERE ProductoId=@id", c =>
        { Parameters(c, entity); SqlExecutor.Add(c, "@id", SqlDbType.Int, entity.ProductoId); });
    }
}
