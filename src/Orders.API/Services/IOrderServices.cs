using System;

namespace Orders.API.Services
{
    using Orders.API.Models;

    public interface IOrderService
    {
        List<Order> GetOrders(Guid? usuarioId);
        Order GetOrder(Guid id);
        // Usamos Task porque la creación consulta la API de productos
        Task<Order> CreateOrderAsync(Order request);
        Order UpdateOrderStatus(Guid id, Order statusUpdate);
    }
}
