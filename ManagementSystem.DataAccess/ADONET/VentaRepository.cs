using System.Data;
using Microsoft.Data.SqlClient;
using ManagementSystem.Domain.Entities;
using ManagementSystem.DataAccess.Interfaces;
namespace ManagementSystem.DataAccess.ADONET;

public class VentaRepository : IVentaRepository
{
    public List<Venta> GetAll() => SqlExecutor.Run(connection =>
    {
        using var command = new SqlCommand("SELECT * FROM dbo.Venta ORDER BY VentaId DESC", connection);
        using var reader = command.ExecuteReader();
        var ventas = new List<Venta>();
        while (reader.Read()) ventas.Add(RowMapper.Venta(reader));
        return ventas;
    });
    public Venta? GetById(int id) => SqlExecutor.Transaction((connection, transaction) => Read(connection, transaction, id));
    private static Venta? Read(SqlConnection connection, SqlTransaction transaction, int id)
    {
        Venta? venta;
        using (var command = new SqlCommand("SELECT * FROM dbo.Venta WITH (UPDLOCK,HOLDLOCK) WHERE VentaId=@id", connection, transaction))
        {
            SqlExecutor.Add(command, "@id", SqlDbType.Int, id);
            using var reader = command.ExecuteReader();
            venta = reader.Read() ? RowMapper.Venta(reader) : null;
        }
        if (venta != null) venta.Detalles = DetalleVentaRepository.Read(connection, transaction, id);
        return venta;
    }
    public int Registrar(Venta venta, bool confirmar)
    {
        ArgumentNullException.ThrowIfNull(venta);
        if (venta.VentaId != 0) throw new ArgumentException("Registrar requiere una venta nueva; para una pendiente use Confirmar.");
        // No confiar en los totales que llega a enviar una pantalla.
        var detalles = venta.Detalles?.Select(d => new DetalleVenta
        {
            ProductoId = d.ProductoId, Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario, Descuento = d.Descuento
        }).ToList() ?? throw new ArgumentException("Faltan detalles.");
        if (detalles.Count == 0) throw new ArgumentException("La venta debe tener al menos un detalle.");
        foreach (var detalle in detalles)
        {
            if (detalle.Cantidad <= 0 || detalle.PrecioUnitario <= 0)
                throw new ArgumentException("Cantidad y precio deben ser positivos.");
            Money(detalle.PrecioUnitario); Money(detalle.Descuento);
            detalle.SubTotal = checked(detalle.Cantidad * detalle.PrecioUnitario);
            if (detalle.Descuento < 0 || detalle.Descuento > detalle.SubTotal)
                throw new ArgumentException("Descuento del detalle inválido.");
        }
        Money(venta.Impuesto); Money(venta.Descuento);
        decimal subtotal = detalles.Sum(d => d.SubTotal);
        decimal descuento = detalles.Sum(d => d.Descuento) + venta.Descuento;
        if (venta.Impuesto < 0 || venta.Descuento < 0 || descuento > subtotal)
            throw new ArgumentException("Impuesto o descuento inválido.");
        decimal total = subtotal + venta.Impuesto - descuento;
        Money(subtotal); Money(descuento); Money(total);
        // El objeto del llamador no se marca como guardado hasta después del commit.
        int id = SqlExecutor.Transaction((connection, transaction) =>
        {
            CheckPerson(connection, transaction, "Cliente", "ClienteId", venta.ClienteId);
            CheckPerson(connection, transaction, "Empleado", "EmpleadoId", venta.EmpleadoId);
            foreach (int productoId in detalles.Select(d => d.ProductoId).Distinct().OrderBy(i => i))
                ReadStock(connection, transaction, productoId, true);
            int ventaId;
            using (var command = new SqlCommand("""
                INSERT dbo.Venta(ClienteId,EmpleadoId,FechaVenta,SubTotal,Impuesto,Descuento,Total,Estado,Observaciones)
                OUTPUT INSERTED.VentaId VALUES(@cliente,@empleado,@fecha,@subtotal,@impuesto,@descuento,@total,N'Pendiente',@observaciones)
                """, connection, transaction))
            {
                SqlExecutor.Add(command, "@cliente", SqlDbType.Int, venta.ClienteId);
                SqlExecutor.Add(command, "@empleado", SqlDbType.Int, venta.EmpleadoId);
                SqlExecutor.Add(command, "@fecha", SqlDbType.DateTime2, venta.FechaVenta);
                SqlExecutor.Add(command, "@subtotal", SqlDbType.Decimal, subtotal);
                SqlExecutor.Add(command, "@impuesto", SqlDbType.Decimal, venta.Impuesto);
                SqlExecutor.Add(command, "@descuento", SqlDbType.Decimal, descuento);
                SqlExecutor.Add(command, "@total", SqlDbType.Decimal, total);
                SqlExecutor.Add(command, "@observaciones", SqlDbType.NVarChar, venta.Observaciones, 500);
                ventaId = Convert.ToInt32(command.ExecuteScalar());
            }
            foreach (var detalle in detalles)
            {
                using var command = new SqlCommand("""
                    INSERT dbo.DetalleVenta(VentaId,ProductoId,Cantidad,PrecioUnitario,Descuento,SubTotal)
                    VALUES(@venta,@producto,@cantidad,@precio,@descuento,@subtotal)
                    """, connection, transaction);
                SqlExecutor.Add(command, "@venta", SqlDbType.Int, ventaId);
                SqlExecutor.Add(command, "@producto", SqlDbType.Int, detalle.ProductoId);
                SqlExecutor.Add(command, "@cantidad", SqlDbType.Int, detalle.Cantidad);
                SqlExecutor.Add(command, "@precio", SqlDbType.Decimal, detalle.PrecioUnitario);
                SqlExecutor.Add(command, "@descuento", SqlDbType.Decimal, detalle.Descuento);
                SqlExecutor.Add(command, "@subtotal", SqlDbType.Decimal, detalle.SubTotal);
                command.ExecuteNonQuery();
            }
            if (confirmar)
            {
                ApplyStock(connection, transaction, ventaId, venta.EmpleadoId, detalles, false);
                SetEstado(connection, transaction, ventaId, "Procesada");
            }
            return ventaId;
        });
        venta.VentaId = id; venta.SubTotal = subtotal; venta.Descuento = descuento;
        venta.Total = total; venta.Estado = confirmar ? "Procesada" : "Pendiente";
        // Detalles originales no reciben IDs: recargar con GetById(id) si se necesitan.
        return id;
    }
    public bool Confirmar(int ventaId) => SqlExecutor.Transaction((connection, transaction) =>
    {
        var venta = Read(connection, transaction, ventaId);
        if (venta == null || venta.Estado != "Pendiente") return false;
        if (venta.Detalles.Count == 0) throw new InvalidOperationException("Venta sin detalles.");
        CheckPerson(connection, transaction, "Cliente", "ClienteId", venta.ClienteId);
        CheckPerson(connection, transaction, "Empleado", "EmpleadoId", venta.EmpleadoId);
        ApplyStock(connection, transaction, ventaId, venta.EmpleadoId, venta.Detalles, false);
        SetEstado(connection, transaction, ventaId, "Procesada");
        return true;
    });
    public bool Anular(int ventaId) => SqlExecutor.Transaction((connection, transaction) =>
    {
        var venta = Read(connection, transaction, ventaId);
        if (venta == null || venta.Estado == "Anulada") return false;
        if (venta.Estado == "Procesada")
            ApplyStock(connection, transaction, ventaId, venta.EmpleadoId, venta.Detalles, true);
        SetEstado(connection, transaction, ventaId, "Anulada");
        return true;
    });
    private static void Money(decimal value)
    {
        if (decimal.Round(value, 2) != value || value > 9999999999999999.99m || value < -9999999999999999.99m)
            throw new ArgumentException("Monto fuera de rango o con más de dos decimales.");
    }
    private static void CheckPerson(SqlConnection connection, SqlTransaction transaction, string table, string key, int id)
    {
        // table/key son constantes internas de este repositorio.
        using var command = new SqlCommand($"SELECT COUNT(*) FROM dbo.[{table}] t JOIN dbo.Persona p ON t.PersonaId=p.PersonaId WHERE t.[{key}]=@id AND t.Activo=1 AND p.Activo=1", connection, transaction);
        SqlExecutor.Add(command, "@id", SqlDbType.Int, id);
        if (Convert.ToInt32(command.ExecuteScalar()) != 1) throw new InvalidOperationException(table + " no disponible.");
    }
    private static int ReadStock(SqlConnection connection, SqlTransaction transaction, int productoId, bool requireActivo)
    {
        using var command = new SqlCommand("SELECT StockActual FROM dbo.Productos WITH (UPDLOCK,HOLDLOCK) WHERE ProductoId=@id" + (requireActivo ? " AND Activo=1" : ""), connection, transaction);
        SqlExecutor.Add(command, "@id", SqlDbType.Int, productoId);
        var value = command.ExecuteScalar();
        if (value == null) throw new InvalidOperationException($"Producto {productoId} no disponible.");
        return Convert.ToInt32(value);
    }
    private static void ApplyStock(SqlConnection connection, SqlTransaction transaction, int ventaId,
        int empleadoId, List<DetalleVenta> detalles, bool devolver)
    {
        // Agrupar impide que dos líneas del mismo producto evadan la comprobación.
        // Ordenar locks reduce el riesgo de interbloqueos entre ventas.
        foreach (var grupo in detalles.GroupBy(d => d.ProductoId).OrderBy(g => g.Key))
        {
            int cantidad = grupo.Sum(d => d.Cantidad);
            int anterior = ReadStock(connection, transaction, grupo.Key, !devolver);
            if (!devolver && anterior < cantidad) throw new InvalidOperationException($"Stock insuficiente para producto {grupo.Key}.");
            int nuevo = checked(devolver ? anterior + cantidad : anterior - cantidad);
            using var command = new SqlCommand("UPDATE dbo.Productos SET StockActual=@stock WHERE ProductoId=@id", connection, transaction);
            SqlExecutor.Add(command, "@stock", SqlDbType.Int, nuevo);
            SqlExecutor.Add(command, "@id", SqlDbType.Int, grupo.Key);
            if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("No se pudo actualizar inventario.");
            MovimientoInventarioRepository.Insert(connection, transaction, grupo.Key, empleadoId,
                devolver ? "Devolución" : "Venta", cantidad, anterior, nuevo,
                $"{(devolver ? "Anulación" : "Confirmación")} venta {ventaId}");
        }
    }
    private static void SetEstado(SqlConnection connection, SqlTransaction transaction, int id, string estado)
    {
        using var command = new SqlCommand("UPDATE dbo.Venta SET Estado=@estado WHERE VentaId=@id", connection, transaction);
        SqlExecutor.Add(command, "@estado", SqlDbType.NVarChar, estado, 20);
        SqlExecutor.Add(command, "@id", SqlDbType.Int, id);
        command.ExecuteNonQuery();
    }
}
