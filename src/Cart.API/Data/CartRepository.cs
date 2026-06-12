namespace Cart.API.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Cart.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

public class CartRepository : ICartRepository
{
    private readonly string _connectionString;

    public CartRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<Cart_> GetByUserIdAsync(Guid userId)
    {
        using var connection = new SqliteConnection(_connectionString);

        // Buscamos la "caja" del carrito
        var cart = await connection.QuerySingleOrDefaultAsync<Cart_>(
            "SELECT * FROM Carts WHERE UsuarioId = @UserId", new { UserId = userId.ToString() });

        if (cart != null)
        {
            // Si existe, le metemos los ítems que tiene adentro
            var items = await connection.QueryAsync<CartItem>(
                "SELECT ProductoId, Cantidad FROM CartItems WHERE UsuarioId = @UserId", new { UserId = userId.ToString() });
            cart.Items = items.ToList();
        }

        return cart!;
    }

    public async Task UpsertCartAsync(Cart_ cart)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Guardamos o actualizamos la fecha del carrito principal
            var sqlCart = @"INSERT INTO Carts (UsuarioId, FechaActualizacion) 
                            VALUES (@UsuarioId, @FechaActualizacion)
                            ON CONFLICT(UsuarioId) DO UPDATE SET FechaActualizacion = @FechaActualizacion";

            await connection.ExecuteAsync(sqlCart, new { UsuarioId = cart.UsuarioId.ToString(), FechaActualizacion = cart.FechaActualizacion.ToString("O") }, transaction);

            // 2. Borramos los ítems viejos
            await connection.ExecuteAsync("DELETE FROM CartItems WHERE UsuarioId = @UsuarioId", new { UsuarioId = cart.UsuarioId.ToString() }, transaction);

            // 3. Insertamos los ítems actuales (si quedó alguno)
            if (cart.Items.Any())
            {
                var sqlItems = "INSERT INTO CartItems (UsuarioId, ProductoId, Cantidad) VALUES (@UsuarioId, @ProductoId, @Cantidad)";
                var itemsToInsert = cart.Items.Select(i => new { UsuarioId = cart.UsuarioId.ToString(), ProductoId = i.ProductoId.ToString(), Cantidad = i.Cantidad });
                await connection.ExecuteAsync(sqlItems, itemsToInsert, transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}