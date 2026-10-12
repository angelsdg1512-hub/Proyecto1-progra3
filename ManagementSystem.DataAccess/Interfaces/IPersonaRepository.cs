using ManagementSystem.Domain.Entities;
namespace ManagementSystem.DataAccess.Interfaces;

public interface IPersonaRepository : IRepository<Persona>
{
    List<Persona> Search(string term);
}
