using System.Data;
using Microsoft.Data.SqlClient;
using ManagementSystem.Domain.Entities;
using ManagementSystem.DataAccess.Interfaces;
namespace ManagementSystem.DataAccess.ADONET;

public class ClienteRepository : RepositoryBase<Cliente>, IClienteRepository
{
    public ClienteRepository() : base("Cliente", "ClienteId", new[] { "PersonaId", "LimiteCredito", "FechaAfiliacion", "Activo" }, true) { }
    protected override int Id(Cliente entity) => entity.ClienteId;
    protected override Cliente Map(SqlDataReader reader) => new()
    {
        ClienteId = reader.GetInt32(reader.GetOrdinal("ClienteId")),
        PersonaId = reader.GetInt32(reader.GetOrdinal("PersonaId")),
        LimiteCredito = reader.GetDecimal(reader.GetOrdinal("LimiteCredito")),
        FechaAfiliacion = reader.GetDateTime(reader.GetOrdinal("FechaAfiliacion")),
        Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
    };
    protected override void Parameters(SqlCommand command, Cliente entity)
    {
        SqlExecutor.Add(command, "@PersonaId", SqlDbType.Int, entity.PersonaId);
        SqlExecutor.Add(command, "@LimiteCredito", SqlDbType.Decimal, entity.LimiteCredito);
        SqlExecutor.Add(command, "@FechaAfiliacion", SqlDbType.DateTime2, entity.FechaAfiliacion);
        SqlExecutor.Add(command, "@Activo", SqlDbType.Bit, entity.Activo);
    }
    public List<Cliente> Search(string term) => Query("WHERE PersonaId IN (SELECT PersonaId FROM dbo.Persona WHERE NombreCompleto LIKE @term OR Identificacion LIKE @term)", c => SqlExecutor.Add(c, "@term", SqlDbType.NVarChar, "%" + term + "%", -1));
}
