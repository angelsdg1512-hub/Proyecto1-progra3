using ManagementSystem.Domain.Entities;
namespace ManagementSystem.DataAccess.Interfaces;

public interface ICategoriaRepository : IRepository<Categoria>
{
    bool EliminarFisicamenteSiNoTieneProductos(int id);
    List<Categoria> GetByEstado(bool activo);
    bool CambiarEstado(int id, bool activo);
}
