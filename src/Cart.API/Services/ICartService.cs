using System;

namespace Cart.API.Services
{
    using Cart.API.Models;
    using System;
    using System.Threading.Tasks;

    public interface ICartService
    {
        Task<ShoppingCart> AddItemAsync(Guid userId, CartItem request);
        Task<ShoppingCart> GetCartAsync(Guid userId);
        Task<ShoppingCart> UpdateItemQuantityAsync(Guid userId, Guid productoId, int cantidad);
        Task<ShoppingCart> RemoveItemAsync(Guid userId, Guid productoId);
    }
}
