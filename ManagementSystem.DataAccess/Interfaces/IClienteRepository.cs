using ManagementSystem.Domain.Entities;
namespace ManagementSystem.DataAccess.Interfaces;

public interface IClienteRepository : IRepository<Cliente>
{
    List<Cliente> Search(string term);
}
