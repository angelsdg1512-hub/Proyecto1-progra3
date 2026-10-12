using System.Data;
using Microsoft.Data.SqlClient;
using ManagementSystem.Domain.Entities;
using ManagementSystem.DataAccess.Interfaces;
namespace ManagementSystem.DataAccess.ADONET;

public class MovimientoInventarioRepository : IMovimientoInventarioRepository
{
    public List<MovimientoInventario> GetAll() => Read(null);
    public List<MovimientoInventario> GetByProducto(int productoId) => Read(productoId);
    private static List<MovimientoInventario> Read(int? productoId) => SqlExecutor.Run(connection =>
    {
        using var command = new SqlCommand("SELECT * FROM dbo.HistoricoMovimientosInventario " +
            (productoId.HasValue ? "WHERE ProductoId=@id " : "") + "ORDER BY MovimientoId DESC", connection);
        if (productoId.HasValue) SqlExecutor.Add(command, "@id", SqlDbType.Int, productoId.Value);
        using var reader = command.ExecuteReader();
        var result = new List<MovimientoInventario>();
        while (reader.Read()) result.Add(RowMapper.MovimientoInventario(reader));
        return result;
    });
    // cambio es firmado: +5 agrega, -5 descuenta. Cantidad en histórico es positiva.
    public int RegistrarCambio(int productoId, int empleadoId, string tipoMovimiento, int cambio, string observacion)
    {
        if (cambio == 0) throw new ArgumentException("El cambio no puede ser cero.");
        if (tipoMovimiento is not ("Entrada" or "Salida" or "Ajuste" or "Devolución"))
            throw new ArgumentException("Tipo inválido; Venta se registra desde VentaRepository.");
        if ((tipoMovimiento is "Entrada" or "Devolución") && cambio < 0)
            throw new ArgumentException("Entrada y devolución deben aumentar el stock.");
        if (tipoMovimiento == "Salida" && cambio > 0)
            throw new ArgumentException("Salida debe disminuir el stock.");
        return SqlExecutor.Transaction((connection, transaction) =>
        {
            using (var employee = new SqlCommand("SELECT COUNT(*) FROM dbo.Empleado WHERE EmpleadoId=@id AND Activo=1", connection, transaction))
            {
                SqlExecutor.Add(employee, "@id", SqlDbType.Int, empleadoId);
                if (Convert.ToInt32(employee.ExecuteScalar()) != 1) throw new InvalidOperationException("Empleado no disponible.");
            }
            int anterior;
            using (var command = new SqlCommand("SELECT StockActual FROM dbo.Productos WITH (UPDLOCK,HOLDLOCK) WHERE ProductoId=@id AND Activo=1", connection, transaction))
            {
                SqlExecutor.Add(command, "@id", SqlDbType.Int, productoId);
                var value = command.ExecuteScalar();
                if (value == null) throw new InvalidOperationException("Producto no disponible.");
                anterior = Convert.ToInt32(value);
            }
            int nuevo = checked(anterior + cambio);
            if (nuevo < 0) throw new InvalidOperationException("Stock insuficiente.");
            using (var command = new SqlCommand("UPDATE dbo.Productos SET StockActual=@stock WHERE ProductoId=@id", connection, transaction))
            {
                SqlExecutor.Add(command, "@stock", SqlDbType.Int, nuevo);
                SqlExecutor.Add(command, "@id", SqlDbType.Int, productoId);
                command.ExecuteNonQuery();
            }
            return Insert(connection, transaction, productoId, empleadoId, tipoMovimiento,
                checked(Math.Abs(cambio)), anterior, nuevo, observacion);
        });
    }
    internal static int Insert(SqlConnection connection, SqlTransaction transaction, int productoId,
        int empleadoId, string tipo, int cantidad, int anterior, int nuevo, string observacion)
    {
        using var command = new SqlCommand("""
            INSERT dbo.HistoricoMovimientosInventario
            (ProductoId,EmpleadoId,FechaMovimiento,TipoMovimiento,Cantidad,StockAnterior,StockNuevo,Observacion)
            OUTPUT INSERTED.MovimientoId
            VALUES(@producto,@empleado,SYSDATETIME(),@tipo,@cantidad,@anterior,@nuevo,@observacion)
            """, connection, transaction);
        SqlExecutor.Add(command, "@producto", SqlDbType.Int, productoId);
        SqlExecutor.Add(command, "@empleado", SqlDbType.Int, empleadoId);
        SqlExecutor.Add(command, "@tipo", SqlDbType.NVarChar, tipo, 20);
        SqlExecutor.Add(command, "@cantidad", SqlDbType.Int, cantidad);
        SqlExecutor.Add(command, "@anterior", SqlDbType.Int, anterior);
        SqlExecutor.Add(command, "@nuevo", SqlDbType.Int, nuevo);
        SqlExecutor.Add(command, "@observacion", SqlDbType.NVarChar, observacion, 500);
        return Convert.ToInt32(command.ExecuteScalar());
    }
}
