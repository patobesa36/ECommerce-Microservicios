using Cart.API.Models;
using Cart.API.Exceptions;
using Cart.API.ExceptionHandlers;

namespace Cart.API.Services
{
    public interface ICartService
    {
        Cart GetCart(Guid userId);
        Cart AddItem(Guid userId, CartItem request);
    }

    public class CartService : ICartService
    {
        private static readonly List<Cart> _carts = new();

        public Cart GetCart(Guid userId)
        {
            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);
            if (cart == null) throw new NotFoundException("CRT-001", "Carrito no encontrado.");
            return cart;
        }

        public Cart AddItem(Guid userId, CartItem request)
        {
            if (request.Cantidad <= 0)
                throw new BadRequestException("CRT-004", "Cantidad inválida.");

            // TODO: Hacer llamada HTTP real a Products.API usando IHttpClientFactory
            // Simulamos la validación
            bool productoExiste = true; // Simulado
            int stockDisponible = 10;   // Simulado

            if (!productoExiste) throw new NotFoundException("CRT-002", "Producto no encontrado.");
            if (stockDisponible < request.Cantidad) throw new UnprocessableEntityException("CRT-003", $"Stock insuficiente. Disponible: {stockDisponible}, solicitado: {request.Cantidad}.");

            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);
            if (cart == null)
            {
                cart = new Cart { UsuarioId = userId };
                _carts.Add(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductoId == request.ProductoId);
            if (existingItem != null) existingItem.Cantidad += request.Cantidad;
            else cart.Items.Add(request);

            cart.FechaActualizacion = DateTime.UtcNow;
            return cart;
        }
    }
}
