using System.Data;
using Microsoft.Data.SqlClient;
using ManagementSystem.Domain.Entities;
using ManagementSystem.DataAccess.Interfaces;
namespace ManagementSystem.DataAccess.ADONET;

public class UsuarioRepository : RepositoryBase<Usuario>, IUsuarioRepository
{
    public UsuarioRepository() : base("Usuarios", "UsuarioId", new[] { "EmpleadoId", "NombreUsuario", "ContrasenaHash", "RolId", "Activo" }, true) { }
    protected override int Id(Usuario entity) => entity.UsuarioId;
    protected override Usuario Map(SqlDataReader reader) => new()
    {
        UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
        EmpleadoId = reader.GetInt32(reader.GetOrdinal("EmpleadoId")),
        NombreUsuario = reader.GetString(reader.GetOrdinal("NombreUsuario")),
        ContrasenaHash = reader.GetString(reader.GetOrdinal("ContrasenaHash")),
        RolId = reader.GetInt32(reader.GetOrdinal("RolId")),
        Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
    };
    protected override void Parameters(SqlCommand command, Usuario entity)
    {
        SqlExecutor.Add(command, "@EmpleadoId", SqlDbType.Int, entity.EmpleadoId);
        SqlExecutor.Add(command, "@NombreUsuario", SqlDbType.NVarChar, entity.NombreUsuario, 100);
        SqlExecutor.Add(command, "@ContrasenaHash", SqlDbType.NVarChar, entity.ContrasenaHash, 500);
        SqlExecutor.Add(command, "@RolId", SqlDbType.Int, entity.RolId);
        SqlExecutor.Add(command, "@Activo", SqlDbType.Bit, entity.Activo);
    }
    // Devuelve el hash almacenado: validar contraseña corresponde a Negocio.
    public Usuario? GetByNombreUsuario(string nombreUsuario) => Query("WHERE NombreUsuario=@nombre AND Activo=1", c => SqlExecutor.Add(c, "@nombre", SqlDbType.NVarChar, nombreUsuario, 100)).FirstOrDefault();
}
