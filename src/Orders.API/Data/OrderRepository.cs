namespace Orders.API.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Orders.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
    {
        using var connection = new SqliteConnection(_connectionString);

        var orders = await connection.QueryAsync<Order>(
            "SELECT * FROM Orders WHERE UsuarioId = @UserId", new { UserId = userId.ToString() });

        foreach (var order in orders)
        {
            var items = await connection.QueryAsync<OrderItem>(
                "SELECT ProductoId, Cantidad, PrecioUnitario FROM OrderItems WHERE OrderId = @OrderId",
                new { OrderId = order.Id.ToString() });
            order.Items = items.ToList();
        }

        return orders;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        using var connection = new SqliteConnection(_connectionString);
        var order = await connection.QuerySingleOrDefaultAsync<Order>(
            "SELECT * FROM Orders WHERE Id = @Id", new { Id = id.ToString() });

        if (order != null)
        {
            var items = await connection.QueryAsync<OrderItem>(
                "SELECT ProductoId, Cantidad, PrecioUnitario FROM OrderItems WHERE OrderId = @OrderId",
                new { OrderId = order.Id.ToString() });
            order.Items = items.ToList();
        }

        return order;
    }

    public async Task CreateAsync(Order order)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var sqlOrder = @"INSERT INTO Orders (Id, UsuarioId, Total, Estado, FechaCreacion) 
                             VALUES (@Id, @UsuarioId, @Total, @Estado, @FechaCreacion)";

            await connection.ExecuteAsync(sqlOrder, new
            {
                Id = order.Id.ToString(),
                UsuarioId = order.UsuarioId.ToString(),
                order.Total,
                order.Estado,
                FechaCreacion = order.FechaCreacion.ToString("O")
            }, transaction);

            if (order.Items != null && order.Items.Any())
            {
                var sqlItems = @"INSERT INTO OrderItems (OrderId, ProductoId, Cantidad, PrecioUnitario) 
                                 VALUES (@OrderId, @ProductoId, @Cantidad, @PrecioUnitario)";

                var itemsToInsert = order.Items.Select(i => new {
                    OrderId = order.Id.ToString(),
                    ProductoId = i.ProductoId.ToString(),
                    i.Cantidad,
                    i.PrecioUnitario
                });

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

    public async Task UpdateStatusAsync(Guid id, string nuevoEstado)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            "UPDATE Orders SET Estado = @Estado WHERE Id = @Id",
            new { Estado = nuevoEstado, Id = id.ToString() });
    }
}