using System.Data;
using Microsoft.Data.SqlClient;
using ManagementSystem.Domain.Entities;
using ManagementSystem.DataAccess.Interfaces;
namespace ManagementSystem.DataAccess.ADONET;

public class PersonaRepository : RepositoryBase<Persona>, IPersonaRepository
{
    public PersonaRepository() : base("Persona", "PersonaId", new[] { "NombreCompleto", "Identificacion", "Email", "Telefono", "Direccion", "FechaRegistro", "Activo" }, true) { }
    protected override int Id(Persona entity) => entity.PersonaId;
    protected override Persona Map(SqlDataReader reader) => new()
    {
        PersonaId = reader.GetInt32(reader.GetOrdinal("PersonaId")),
        NombreCompleto = reader.GetString(reader.GetOrdinal("NombreCompleto")),
        Identificacion = reader.GetString(reader.GetOrdinal("Identificacion")),
        Email = reader.GetString(reader.GetOrdinal("Email")),
        Telefono = reader.GetString(reader.GetOrdinal("Telefono")),
        Direccion = reader.GetString(reader.GetOrdinal("Direccion")),
        FechaRegistro = reader.GetDateTime(reader.GetOrdinal("FechaRegistro")),
        Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
    };
    protected override void Parameters(SqlCommand command, Persona entity)
    {
        SqlExecutor.Add(command, "@NombreCompleto", SqlDbType.NVarChar, entity.NombreCompleto, 200);
        SqlExecutor.Add(command, "@Identificacion", SqlDbType.NVarChar, entity.Identificacion, 50);
        SqlExecutor.Add(command, "@Email", SqlDbType.NVarChar, entity.Email, 200);
        SqlExecutor.Add(command, "@Telefono", SqlDbType.NVarChar, entity.Telefono, 30);
        SqlExecutor.Add(command, "@Direccion", SqlDbType.NVarChar, entity.Direccion, 500);
        SqlExecutor.Add(command, "@FechaRegistro", SqlDbType.DateTime2, entity.FechaRegistro);
        SqlExecutor.Add(command, "@Activo", SqlDbType.Bit, entity.Activo);
    }
    public List<Persona> Search(string term) => Query("WHERE NombreCompleto LIKE @term OR Identificacion LIKE @term", c => SqlExecutor.Add(c, "@term", SqlDbType.NVarChar, "%" + term + "%", -1));
}
