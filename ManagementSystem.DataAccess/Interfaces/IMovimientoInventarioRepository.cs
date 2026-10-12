using ManagementSystem.Domain.Entities;
namespace ManagementSystem.DataAccess.Interfaces;
public interface IMovimientoInventarioRepository
{
    List<MovimientoInventario> GetAll();
    List<MovimientoInventario> GetByProducto(int productoId);
    int RegistrarCambio(int productoId, int empleadoId, string tipoMovimiento, int cambio, string observacion);
}
