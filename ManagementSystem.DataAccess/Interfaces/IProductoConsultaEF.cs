using ManagementSystem.Domain.Entities;
namespace ManagementSystem.DataAccess.Interfaces;
public interface IProductoConsultaEF
{
    List<Producto> GetByCategoria(int categoriaId, bool soloActivos = true);
    List<Producto> GetStockBajo();
}
