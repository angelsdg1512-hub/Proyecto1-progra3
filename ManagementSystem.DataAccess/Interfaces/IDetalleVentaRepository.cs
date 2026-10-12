using ManagementSystem.Domain.Entities;
namespace ManagementSystem.DataAccess.Interfaces;
public interface IDetalleVentaRepository
{
    List<DetalleVenta> GetByVenta(int ventaId);
}
