using ManagementSystem.Domain.Entities;
namespace ManagementSystem.DataAccess.Interfaces;

public interface IEmpleadoRepository : IRepository<Empleado>
{
    List<Empleado> Search(string term);
}
