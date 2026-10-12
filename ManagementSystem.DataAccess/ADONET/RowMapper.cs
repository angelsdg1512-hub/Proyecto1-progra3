using Microsoft.Data.SqlClient;
using ManagementSystem.Domain.Entities;
namespace ManagementSystem.DataAccess.ADONET;
internal static class RowMapper
{
    internal static Venta Venta(SqlDataReader reader) => new()
    {
        VentaId = reader.GetInt32(reader.GetOrdinal("VentaId")),
        ClienteId = reader.GetInt32(reader.GetOrdinal("ClienteId")),
        EmpleadoId = reader.GetInt32(reader.GetOrdinal("EmpleadoId")),
        FechaVenta = reader.GetDateTime(reader.GetOrdinal("FechaVenta")),
        SubTotal = reader.GetDecimal(reader.GetOrdinal("SubTotal")),
        Impuesto = reader.GetDecimal(reader.GetOrdinal("Impuesto")),
        Descuento = reader.GetDecimal(reader.GetOrdinal("Descuento")),
        Total = reader.GetDecimal(reader.GetOrdinal("Total")),
        Estado = reader.GetString(reader.GetOrdinal("Estado")),
        Observaciones = reader.GetString(reader.GetOrdinal("Observaciones")),
    };
    internal static DetalleVenta DetalleVenta(SqlDataReader reader) => new()
    {
        DetalleVentaId = reader.GetInt32(reader.GetOrdinal("DetalleVentaId")),
        VentaId = reader.GetInt32(reader.GetOrdinal("VentaId")),
        ProductoId = reader.GetInt32(reader.GetOrdinal("ProductoId")),
        Cantidad = reader.GetInt32(reader.GetOrdinal("Cantidad")),
        PrecioUnitario = reader.GetDecimal(reader.GetOrdinal("PrecioUnitario")),
        Descuento = reader.GetDecimal(reader.GetOrdinal("Descuento")),
        SubTotal = reader.GetDecimal(reader.GetOrdinal("SubTotal")),
    };
    internal static MovimientoInventario MovimientoInventario(SqlDataReader reader) => new()
    {
        MovimientoId = reader.GetInt32(reader.GetOrdinal("MovimientoId")),
        ProductoId = reader.GetInt32(reader.GetOrdinal("ProductoId")),
        EmpleadoId = reader.GetInt32(reader.GetOrdinal("EmpleadoId")),
        FechaMovimiento = reader.GetDateTime(reader.GetOrdinal("FechaMovimiento")),
        TipoMovimiento = reader.GetString(reader.GetOrdinal("TipoMovimiento")),
        Cantidad = reader.GetInt32(reader.GetOrdinal("Cantidad")),
        StockAnterior = reader.GetInt32(reader.GetOrdinal("StockAnterior")),
        StockNuevo = reader.GetInt32(reader.GetOrdinal("StockNuevo")),
        Observacion = reader.GetString(reader.GetOrdinal("Observacion")),
    };
}
