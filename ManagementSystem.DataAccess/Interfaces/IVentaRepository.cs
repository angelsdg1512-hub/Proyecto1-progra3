using ManagementSystem.Domain.Entities;
namespace ManagementSystem.DataAccess.Interfaces;

// Las ventas y sus detalles se escriben juntos: no exponer CRUD independiente.
public interface IVentaRepository
{
    List<Venta> GetAll(); // Encabezados sin detalles; usar GetById para el agregado completo.
    Venta? GetById(int id);
    int Registrar(Venta venta, bool confirmar);
    bool Confirmar(int ventaId);
    bool Anular(int ventaId);
}
