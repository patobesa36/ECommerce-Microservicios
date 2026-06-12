namespace Orders.API.Data;
using Orders.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
    Task<Order?> GetByIdAsync(Guid id);
    Task CreateAsync(Order order);
    Task UpdateStatusAsync(Guid id, string nuevoEstado);
}