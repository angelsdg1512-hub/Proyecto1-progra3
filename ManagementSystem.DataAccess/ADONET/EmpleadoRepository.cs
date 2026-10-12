using System.Data;
using Microsoft.Data.SqlClient;
using ManagementSystem.Domain.Entities;
using ManagementSystem.DataAccess.Interfaces;
namespace ManagementSystem.DataAccess.ADONET;

public class EmpleadoRepository : RepositoryBase<Empleado>, IEmpleadoRepository
{
    public EmpleadoRepository() : base("Empleado", "EmpleadoId", new[] { "PersonaId", "Puesto", "Salario", "FechaIngreso", "Activo" }, true) { }
    protected override int Id(Empleado entity) => entity.EmpleadoId;
    protected override Empleado Map(SqlDataReader reader) => new()
    {
        EmpleadoId = reader.GetInt32(reader.GetOrdinal("EmpleadoId")),
        PersonaId = reader.GetInt32(reader.GetOrdinal("PersonaId")),
        Puesto = reader.GetString(reader.GetOrdinal("Puesto")),
        Salario = reader.GetDecimal(reader.GetOrdinal("Salario")),
        FechaIngreso = reader.GetDateTime(reader.GetOrdinal("FechaIngreso")),
        Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
    };
    protected override void Parameters(SqlCommand command, Empleado entity)
    {
        SqlExecutor.Add(command, "@PersonaId", SqlDbType.Int, entity.PersonaId);
        SqlExecutor.Add(command, "@Puesto", SqlDbType.NVarChar, entity.Puesto, 100);
        SqlExecutor.Add(command, "@Salario", SqlDbType.Decimal, entity.Salario);
        SqlExecutor.Add(command, "@FechaIngreso", SqlDbType.DateTime2, entity.FechaIngreso);
        SqlExecutor.Add(command, "@Activo", SqlDbType.Bit, entity.Activo);
    }
    public List<Empleado> Search(string term) => Query("WHERE PersonaId IN (SELECT PersonaId FROM dbo.Persona WHERE NombreCompleto LIKE @term OR Identificacion LIKE @term)", c => SqlExecutor.Add(c, "@term", SqlDbType.NVarChar, "%" + term + "%", -1));
}
