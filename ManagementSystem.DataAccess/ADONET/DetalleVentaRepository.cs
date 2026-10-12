using System.Data;
using Microsoft.Data.SqlClient;
using ManagementSystem.Domain.Entities;
using ManagementSystem.DataAccess.Interfaces;
namespace ManagementSystem.DataAccess.ADONET;
public class DetalleVentaRepository : IDetalleVentaRepository
{
    public List<DetalleVenta> GetByVenta(int ventaId) => SqlExecutor.Run(c => Read(c, null, ventaId));
    internal static List<DetalleVenta> Read(SqlConnection connection, SqlTransaction? transaction, int ventaId)
    {
        using var command = new SqlCommand("SELECT * FROM dbo.DetalleVenta WHERE VentaId=@id ORDER BY DetalleVentaId", connection, transaction);
        SqlExecutor.Add(command, "@id", SqlDbType.Int, ventaId);
        using var reader = command.ExecuteReader();
        var result = new List<DetalleVenta>();
        while (reader.Read()) result.Add(RowMapper.DetalleVenta(reader));
        return result;
    }
}
