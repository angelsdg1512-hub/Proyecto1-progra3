using System.Data;
using Microsoft.Data.SqlClient;
using ManagementSystem.Domain.Entities;
using ManagementSystem.DataAccess.Interfaces;
namespace ManagementSystem.DataAccess.ADONET;

public class RolRepository : RepositoryBase<Rol>, IRolRepository
{
    public RolRepository() : base("Roles", "RolId", new[] { "NombreRol", "Descripcion", "Activo" }, true) { }
    protected override int Id(Rol entity) => entity.RolId;
    protected override Rol Map(SqlDataReader reader) => new()
    {
        RolId = reader.GetInt32(reader.GetOrdinal("RolId")),
        NombreRol = reader.GetString(reader.GetOrdinal("NombreRol")),
        Descripcion = reader.GetString(reader.GetOrdinal("Descripcion")),
        Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
    };
    protected override void Parameters(SqlCommand command, Rol entity)
    {
        SqlExecutor.Add(command, "@NombreRol", SqlDbType.NVarChar, entity.NombreRol, 50);
        SqlExecutor.Add(command, "@Descripcion", SqlDbType.NVarChar, entity.Descripcion, 500);
        SqlExecutor.Add(command, "@Activo", SqlDbType.Bit, entity.Activo);
    }
}
