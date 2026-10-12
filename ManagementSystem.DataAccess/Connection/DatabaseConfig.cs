using System;
using System.Configuration;
using Microsoft.Data.SqlClient;

namespace ManagementSystem.DataAccess.Connection;

// Código proporcionado por el profesor: conservamos su comportamiento.
public static class DatabaseConfig
{
    private static string _connectionString = "Server=.;Database=tienda_db;Integrated Security=True;TrustServerCertificate=True;";
    static DatabaseConfig()
    {
        try
        {
            var connStrFromConfig = ConfigurationManager.ConnectionStrings["FacturacionConnection"]?.ConnectionString;
            if (!string.IsNullOrWhiteSpace(connStrFromConfig)) _connectionString = connStrFromConfig;
        }
        catch { /* Fallback a la cadena predeterminada. */ }
    }
    public static string ConnectionString
    {
        get => _connectionString;
        set { if (!string.IsNullOrWhiteSpace(value)) _connectionString = value; }
    }
    public static SqlConnection CreateConnection() => new SqlConnection(_connectionString);
    public static (bool Success, string Message) TestConnection()
    {
        try
        {
            using var conn = CreateConnection();
            conn.Open();
            return (true, "¡Conexión establecida con éxito con SQL Server!");
        }
        catch (Exception ex) { return (false, $"Error al conectar con SQL Server: {ex.Message}"); }
    }
}
