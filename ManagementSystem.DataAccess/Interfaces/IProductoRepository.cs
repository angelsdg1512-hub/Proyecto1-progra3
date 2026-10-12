using ManagementSystem.Domain.Entities;
namespace ManagementSystem.DataAccess.Interfaces;

// Firmas originales del profesor, sin agregar herencia al contrato.
public interface IProductoRepository
{
    List<Producto> GetAll();
    Producto? GetById(int id);
    int Create(Producto producto);
    bool Update(Producto producto);
    bool Delete(int id);
    List<Producto> Search(string term);
    bool UpdateStock(int idProducto, int cantidadADescontar);
}
