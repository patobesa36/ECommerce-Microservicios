using Orders.API.DTOs;
using Orders.API.Models;

namespace Orders.API.Services
{
    using Orders.API.DTOs;
    using Orders.API.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IOrderServices
    {
        Task<IEnumerable<Order>> GetOrdersAsync(Guid userId); 
        Task<Order> GetOrderAsync(Guid id);                    
        Task<Order> CreateOrderAsync(Order request);          
        Task<Order> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusDto request); 
    }
}
