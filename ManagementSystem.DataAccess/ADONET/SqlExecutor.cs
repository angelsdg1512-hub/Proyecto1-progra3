using System.Data;
using Microsoft.Data.SqlClient;
using ManagementSystem.DataAccess.Connection;
namespace ManagementSystem.DataAccess.ADONET;

// Centraliza abrir/cerrar. El finally satisface la indicación explícita del PDF.
internal static class SqlExecutor
{
    internal static T Run<T>(Func<SqlConnection, T> operation)
    {
        using var connection = DatabaseConfig.CreateConnection();
        try { connection.Open(); return operation(connection); }
        finally { connection.Close(); }
    }
    internal static T Transaction<T>(Func<SqlConnection, SqlTransaction, T> operation)
    {
        return Run(connection =>
        {
            using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                var result = operation(connection, transaction);
                transaction.Commit();
                return result;
            }
            catch (Exception original)
            {
                try { transaction.Rollback(); }
                catch (Exception rollbackError)
                { throw new AggregateException("Fallaron la operación y el rollback.", original, rollbackError); }
                throw;
            }
        });
    }
    internal static void Add(SqlCommand command, string name, SqlDbType type, object value, int size = 0)
    {
        var parameter = command.Parameters.Add(name, type);
        if (size > 0) parameter.Size = size;
        if (type == SqlDbType.Decimal) { parameter.Precision = 18; parameter.Scale = 2; }
        parameter.Value = value ?? DBNull.Value;
    }
}
