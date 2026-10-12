using System.Data;
using Microsoft.Data.SqlClient;
using ManagementSystem.Domain.Entities;
using ManagementSystem.DataAccess.Interfaces;
namespace ManagementSystem.DataAccess.ADONET;

public class CategoriaRepository : RepositoryBase<Categoria>, ICategoriaRepository
{
    public CategoriaRepository() : base("Categorias", "CategoriaId", new[] { "Nombre", "Descripcion", "FechaCreacion", "Activo" }, true) { }
    protected override int Id(Categoria entity) => entity.CategoriaId;
    protected override Categoria Map(SqlDataReader reader) => new()
    {
        CategoriaId = reader.GetInt32(reader.GetOrdinal("CategoriaId")),
        Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
        Descripcion = reader.GetString(reader.GetOrdinal("Descripcion")),
        FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
        Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
    };
    protected override void Parameters(SqlCommand command, Categoria entity)
    {
        SqlExecutor.Add(command, "@Nombre", SqlDbType.NVarChar, entity.Nombre, 150);
        SqlExecutor.Add(command, "@Descripcion", SqlDbType.NVarChar, entity.Descripcion, 500);
        SqlExecutor.Add(command, "@FechaCreacion", SqlDbType.DateTime2, entity.FechaCreacion);
        SqlExecutor.Add(command, "@Activo", SqlDbType.Bit, entity.Activo);
    }
    // DELETE real exigido por el PDF, distinto del Delete lógico habitual.
    // Usar sólo para categorías inactivas y sin ningún producto relacionado.
    public bool EliminarFisicamenteSiNoTieneProductos(int id) => Execute(
        "DELETE FROM dbo.Categorias WHERE CategoriaId=@id AND Activo=0 AND NOT EXISTS (SELECT 1 FROM dbo.Productos WHERE CategoriaId=@id)",
        c => SqlExecutor.Add(c, "@id", SqlDbType.Int, id));
    public List<Categoria> GetByEstado(bool activo) => Query("WHERE Activo=@activo", c => SqlExecutor.Add(c, "@activo", SqlDbType.Bit, activo));
    public bool CambiarEstado(int id, bool activo) => Execute("UPDATE dbo.Categorias SET Activo=@activo WHERE CategoriaId=@id", c =>
    { SqlExecutor.Add(c, "@activo", SqlDbType.Bit, activo); SqlExecutor.Add(c, "@id", SqlDbType.Int, id); });
}
