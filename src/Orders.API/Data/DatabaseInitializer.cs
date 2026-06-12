namespace Orders.API.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

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
            CREATE TABLE IF NOT EXISTS Orders (
                Id TEXT PRIMARY KEY,
                UsuarioId TEXT NOT NULL,
                Total REAL NOT NULL,
                Estado TEXT NOT NULL,
                FechaCreacion TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS OrderItems (
                OrderId TEXT NOT NULL,
                ProductoId TEXT NOT NULL,
                Cantidad INTEGER NOT NULL,
                PrecioUnitario REAL NOT NULL,
                FOREIGN KEY(OrderId) REFERENCES Orders(Id) ON DELETE CASCADE
            );";

        connection.Execute(sql);
    }
}