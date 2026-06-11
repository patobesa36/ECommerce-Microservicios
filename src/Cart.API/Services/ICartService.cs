using System;

namespace Cart.API.Services
{
    using Cart.API.Models;
    using System;
    using System.Threading.Tasks;

    public interface ICartService
    {
        // Usamos Task porque las llamadas HTTP son asíncronas
        Task<Cart_> AddItemAsync(Guid userId, CartItem request);
    }
}
