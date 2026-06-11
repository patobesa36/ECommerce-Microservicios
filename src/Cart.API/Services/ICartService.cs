using System;

namespace Cart.API.Services
{
    using Cart.API.Models;
    using System;
    using System.Threading.Tasks;

    public interface ICartService
    {
        Task<Cart_> AddItemAsync(Guid userId, CartItem request);
        Task<Cart_> GetCartAsync(Guid userId);
        Task<Cart_> UpdateItemQuantityAsync(Guid userId, Guid productoId, int cantidad);
        Task<Cart_> RemoveItemAsync(Guid userId, Guid productoId);
    }
}
