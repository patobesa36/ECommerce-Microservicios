namespace Products.API.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Products.API.Models;
using System.Data;

public class ProductRepository : IProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(string? categoria, string? nombre)
    {
        using var connection = new SqliteConnection(_connectionString);

        // Armamos la query dinámica
        var sql = "SELECT * FROM Products WHERE 1=1";
        if (!string.IsNullOrEmpty(categoria)) sql += " AND Categoria = @Categoria";
        if (!string.IsNullOrEmpty(nombre)) sql += " AND Nombre LIKE '%' || @Nombre || '%'";

        return await connection.QueryAsync<Product>(sql, new { Categoria = categoria, Nombre = nombre });
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        using var connection = new SqliteConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<Product>(
            "SELECT * FROM Products WHERE Id = @Id", new { Id = id.ToString() });
    }

    public async Task<Product?> GetByNameAsync(string nombre)
    {
        using var connection = new SqliteConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<Product>(
            "SELECT * FROM Products WHERE LOWER(Nombre) = LOWER(@Nombre)", new { Nombre = nombre });
    }

    public async Task CreateAsync(Product product)
    {
        using var connection = new SqliteConnection(_connectionString);
        var sql = @"INSERT INTO Products (Id, Nombre, Descripcion, Precio, Stock, Categoria, FechaCreacion) 
                    VALUES (@Id, @Nombre, @Descripcion, @Precio, @Stock, @Categoria, @FechaCreacion)";

        await connection.ExecuteAsync(sql, new
        {
            Id = product.Id.ToString(),
            product.Nombre,
            product.Descripcion,
            product.Precio,
            product.Stock,
            product.Categoria,
            FechaCreacion = product.FechaCreacion.ToString("O")
        });
    }

    public async Task UpdateAsync(Product product)
    {
        using var connection = new SqliteConnection(_connectionString);
        var sql = @"UPDATE Products 
                    SET Nombre = @Nombre, Descripcion = @Descripcion, Precio = @Precio, 
                        Stock = @Stock, Categoria = @Categoria 
                    WHERE Id = @Id";

        await connection.ExecuteAsync(sql, new
        {
            product.Nombre,
            product.Descripcion,
            product.Precio,
            product.Stock,
            product.Categoria,
            Id = product.Id.ToString()
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync("DELETE FROM Products WHERE Id = @Id", new { Id = id.ToString() });
    }
}
