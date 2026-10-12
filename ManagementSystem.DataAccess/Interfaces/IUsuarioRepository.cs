using ManagementSystem.Domain.Entities;
namespace ManagementSystem.DataAccess.Interfaces;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Usuario? GetByNombreUsuario(string nombreUsuario);
}
