namespace Cart.API.Services
{
    using Cart.API.Models;
    using Cart.API.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Cart.API.DTOs;

    public class CartService : ICartService
    {
        private static readonly List<Cart_> _carts = new();
        private readonly HttpClient _httpClient;

        public CartService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // Ajustar el puerto según tu Products.API
            _httpClient.BaseAddress = new Uri("https://localhost:7001/");
        }

        public async Task<Cart_> AddItemAsync(Guid userId, CartItem request)
        {
            if (request.Cantidad <= 0)
                throw new BusinessRuleException("CRT-004", "Cantidad inválida. Debe ser mayor a 0.");

            // Validar producto y stock en Products.API
            var product = await GetProductFromCatalogAsync(request.ProductoId);

            if (product.Stock < request.Cantidad)
                throw new BusinessRuleException("CRT-003", $"Stock insuficiente. Disponible: {product.Stock}, solicitado: {request.Cantidad}.");

            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);
            if (cart == null)
            {
                cart = new Cart_ { UsuarioId = userId };
                _carts.Add(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductoId == request.ProductoId);
            if (existingItem != null)
            {
                if (product.Stock < (existingItem.Cantidad + request.Cantidad))
                    throw new BusinessRuleException("CRT-003", $"Stock insuficiente acumulado. Disponible: {product.Stock}, en carrito: {existingItem.Cantidad}, solicitado extra: {request.Cantidad}.");

                existingItem.Cantidad += request.Cantidad;
            }
            else
            {
                cart.Items.Add(request);
            }

            cart.FechaActualizacion = DateTime.UtcNow;
            return cart;
        }

        public async Task<Cart_> GetCartAsync(Guid userId)
        {
            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);
            // El catálogo exige CRT-001 si el carrito está vacío o no existe
            if (cart == null || !cart.Items.Any())
                throw new NotFoundException("CRT-001", "El carrito solicitado no contiene elementos.");

            return await Task.FromResult(cart);
        }

        public async Task<Cart_> UpdateItemQuantityAsync(Guid userId, Guid productoId, int cantidad)
        {
            if (cantidad <= 0)
                throw new BusinessRuleException("CRT-004", "Cantidad inválida. Debe ser mayor a 0.");

            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);
            if (cart == null)
                throw new NotFoundException("CRT-001", "El carrito solicitado no existe.");

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductoId == productoId);
            if (existingItem == null)
                throw new NotFoundException("CRT-002", "El producto no se encuentra en el carrito.");

            // Validamos stock en la API de Productos para la nueva cantidad fija
            var product = await GetProductFromCatalogAsync(productoId);
            if (product.Stock < cantidad)
                throw new BusinessRuleException("CRT-003", $"Stock insuficiente. Disponible: {product.Stock}, solicitado: {cantidad}.");

            existingItem.Cantidad = cantidad;
            cart.FechaActualizacion = DateTime.UtcNow;

            return cart;
        }

        public async Task<Cart_> RemoveItemAsync(Guid userId, Guid productoId)
        {
            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);
            if (cart == null)
                throw new NotFoundException("CRT-001", "El carrito solicitado no existe.");

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductoId == productoId);
            if (existingItem == null)
                throw new NotFoundException("CRT-002", "El producto no se encuentra en el carrito.");

            cart.Items.Remove(existingItem);
            cart.FechaActualizacion = DateTime.UtcNow;

            return cart;
        }

        // Método auxiliar privado para reutilizar la llamada HTTP a Productos
        private async Task<ProductDto> GetProductFromCatalogAsync(Guid productoId)
        {
            var response = await _httpClient.GetAsync($"api/products/{productoId}");
            if (!response.IsSuccessStatusCode)
                throw new NotFoundException("CRT-002", "Producto no encontrado en el catálogo.");

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ProductDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

      
    }
}