using Microsoft.Data.SqlClient;
using System.Data;
using ManagementSystem.DataAccess.Interfaces;
namespace ManagementSystem.DataAccess.ADONET;

public abstract class RepositoryBase<T> : IRepository<T> where T : class
{
    private readonly string table;
    private readonly string idColumn;
    private readonly string[] columns;
    private readonly bool logicalDelete;
    protected RepositoryBase(string table, string idColumn, string[] columns, bool logicalDelete)
    { this.table = table; this.idColumn = idColumn; this.columns = columns; this.logicalDelete = logicalDelete; }
    protected abstract T Map(SqlDataReader reader);
    protected abstract void Parameters(SqlCommand command, T entity);
    protected abstract int Id(T entity);
    // where/order son fragmentos internos fijos, nunca texto recibido de una pantalla.
    protected List<T> Query(string where = "", Action<SqlCommand>? configure = null)
    {
        return SqlExecutor.Run(connection =>
        {
            using var command = new SqlCommand($"SELECT [{idColumn}],{string.Join(",", columns.Select(c => $"[{c}]"))} FROM dbo.[{table}] {where} ORDER BY [{idColumn}]", connection);
            configure?.Invoke(command);
            using var reader = command.ExecuteReader();
            var result = new List<T>();
            while (reader.Read()) result.Add(Map(reader));
            return result;
        });
    }
    protected bool Execute(string sql, Action<SqlCommand> configure)
    {
        return SqlExecutor.Run(connection =>
        {
            using var command = new SqlCommand(sql, connection);
            configure(command);
            return command.ExecuteNonQuery() == 1;
        });
    }
    public virtual List<T> GetAll() => Query(); // Incluye inactivos para administración.
    public virtual T? GetById(int id) => Query($"WHERE [{idColumn}]=@id", c => SqlExecutor.Add(c, "@id", SqlDbType.Int, id)).FirstOrDefault();
    public virtual int Create(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return SqlExecutor.Run(connection =>
        {
            string names = string.Join(",", columns.Select(c => $"[{c}]"));
            string values = string.Join(",", columns.Select(c => "@" + c));
            using var command = new SqlCommand($"INSERT INTO dbo.[{table}] ({names}) OUTPUT INSERTED.[{idColumn}] VALUES ({values});", connection);
            Parameters(command, entity);
            return Convert.ToInt32(command.ExecuteScalar());
        });
    }
    public virtual bool Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        string assignments = string.Join(",", columns.Select(c => $"[{c}]=@{c}"));
        return Execute($"UPDATE dbo.[{table}] SET {assignments} WHERE [{idColumn}]=@id", c =>
        { Parameters(c, entity); SqlExecutor.Add(c, "@id", SqlDbType.Int, Id(entity)); });
    }
    public virtual bool Delete(int id)
    {
        string sql = logicalDelete ? $"UPDATE dbo.[{table}] SET Activo=0 WHERE [{idColumn}]=@id AND Activo=1"
                                  : $"DELETE FROM dbo.[{table}] WHERE [{idColumn}]=@id";
        return Execute(sql, c => SqlExecutor.Add(c, "@id", SqlDbType.Int, id));
    }
}
