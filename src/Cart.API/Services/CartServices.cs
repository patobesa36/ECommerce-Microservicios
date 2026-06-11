using Cart.API.Models;
using Cart.API.Exceptions;
using Cart.API.ExceptionHandlers;

namespace Cart.API.Services
{
    using Cart.API.Models;
    using Cart.API.Exceptions;
    using System.Text.Json;

    public class CartService : ICartService
    {
        // Movemos la lista simulada acá (hasta que usen la librería de la cátedra)
        private static readonly List<Cart_> _carts = new();

        private readonly HttpClient _httpClient;

        // Inyectamos el HttpClient en el constructor
        public CartService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Configuramos la URL base de tu Products.API (Ajustá el puerto según tu entorno)
            _httpClient.BaseAddress = new Uri("https://localhost:7001/");
        }

        public async Task<Cart_> AddItemAsync(Guid userId, CartItem request)
        {
            if (request.Cantidad <= 0)
                throw new BusinessRuleException("CRT-004", "Cantidad inválida.");

            // 1. LLAMADA HTTP A PRODUCTS API
            // Le pegamos al endpoint GET /api/products/{id}
            var response = await _httpClient.GetAsync($"api/products/{request.ProductoId}");

            if (!response.IsSuccessStatusCode)
            {
                // Si el producto no existe en la API de Productos, lanzamos el error del Carrito
                throw new NotFoundException("CRT-002", "Producto no encontrado.");
            }

            // 2. DESERIALIZAR LA RESPUESTA PARA VALIDAR STOCK
            var productContent = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<ProductDto>(productContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (product == null || product.Stock < request.Cantidad)
            {
                throw new BusinessRuleException("CRT-003", $"Stock insuficiente. Disponible: {product?.Stock}, solicitado: {request.Cantidad}.");
            }

            // 3. LÓGICA DE AGREGAR AL CARRITO (Lo que ya tenías)
            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);
            if (cart == null)
            {
                cart = new Cart_ { UsuarioId = userId };
                _carts.Add(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductoId == request.ProductoId);
            if (existingItem != null)
                existingItem.Cantidad += request.Cantidad;
            else
                cart.Items.Add(request);

            cart.FechaActualizacion = DateTime.UtcNow;

            return cart;
        }

        // Clase auxiliar interna para leer la respuesta de Products API
        private class ProductDto
        {
            public Guid Id { get; set; }
            public int Stock { get; set; }
            public decimal Precio { get; set; }
        }
    }
}