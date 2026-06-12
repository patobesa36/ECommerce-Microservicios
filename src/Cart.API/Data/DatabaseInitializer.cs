namespace Cart.API.Data;
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
            CREATE TABLE IF NOT EXISTS Carts (
                UsuarioId TEXT PRIMARY KEY,
                FechaActualizacion TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS CartItems (
                UsuarioId TEXT NOT NULL,
                ProductoId TEXT NOT NULL,
                Cantidad INTEGER NOT NULL,
                FOREIGN KEY(UsuarioId) REFERENCES Carts(UsuarioId) ON DELETE CASCADE
            );";

        connection.Execute(sql);
    }
}