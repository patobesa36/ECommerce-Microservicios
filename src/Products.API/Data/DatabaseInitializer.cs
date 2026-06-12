namespace Products.API.Data;
using Dapper;
using Microsoft.Data.Sqlite;

public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var sql = @"
            CREATE TABLE IF NOT EXISTS Products (
                Id TEXT PRIMARY KEY,
                Nombre TEXT NOT NULL,
                Descripcion TEXT NOT NULL,
                Precio REAL NOT NULL,
                Stock INTEGER NOT NULL,
                Categoria TEXT NOT NULL,
                FechaCreacion TEXT NOT NULL
            )";

        connection.Execute(sql);
    }
}