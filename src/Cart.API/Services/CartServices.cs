namespace Cart.API.Services
{
    using Cart.API.Models;
    using Cart.API.Exceptions;
    using Cart.API.DTOs;
    using Cart.API.Data;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class CartService : ICartService
    {
        // 1. Reemplazamos la lista estática por nuestro repositorio real
        private readonly ICartRepository _cartRepository;
        private readonly HttpClient _productsHttpClient;

        public CartService(HttpClient productsHttpClient, ICartRepository cartRepository)
        {
            _productsHttpClient = productsHttpClient;
            // Recordá mapear el puerto exacto de tu Products.API
            _productsHttpClient.BaseAddress = new Uri("https://localhost:7001/");

            _cartRepository = cartRepository;
        }

        public async Task<Cart_> AddItemAsync(Guid userId, CartItem request)
        {
            if (request.Cantidad <= 0)
                throw new BusinessRuleException("CRT-004", "Cantidad inválida. Debe ser mayor a 0.");

            // Validar producto y stock comunicándonos por HTTP con la API de Productos
            var product = await GetProductFromCatalogAsync(request.ProductoId);

            if (product.Stock < request.Cantidad)
                throw new BusinessRuleException("CRT-003", $"Stock insuficiente. Disponible: {product.Stock}, solicitado: {request.Cantidad}.");

            // Buscamos si el usuario ya tiene un carrito guardado en SQLite
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart_ { UsuarioId = userId, Items = new List<CartItem>() };
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

            // Guardamos o actualizamos en la base de datos de forma relacional
            await _cartRepository.UpsertCartAsync(cart);
            return cart;
        }

        public async Task<Cart_> GetCartAsync(Guid userId)
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);

            // El catálogo de errores exige CRT-001 si el carrito no tiene elementos o no existe
            if (cart == null || cart.Items == null || !cart.Items.Any())
                throw new NotFoundException("CRT-001", "El carrito solicitado no contiene elementos.");

            return cart;
        }

        public async Task<Cart_> UpdateItemQuantityAsync(Guid userId, Guid productoId, int cantidad)
        {
            if (cantidad <= 0)
                throw new BusinessRuleException("CRT-004", "Cantidad inválida. Debe ser mayor a 0.");

            var cart = await _cartRepository.GetByUserIdAsync(userId);
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

            // Mandamos los cambios actualizados a SQLite
            await _cartRepository.UpsertCartAsync(cart);
            return cart;
        }

        public async Task<Cart_> RemoveItemAsync(Guid userId, Guid productoId)
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null)
                throw new NotFoundException("CRT-001", "El carrito solicitado no existe.");

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductoId == productoId);
            if (existingItem == null)
                throw new NotFoundException("CRT-002", "El producto no se encuentra en el carrito.");

            cart.Items.Remove(existingItem);
            cart.FechaActualizacion = DateTime.UtcNow;

            // Persistimos en la base de datos (si el carrito quedó vacío, Upsert borrará todos sus ítems en la BD)
            await _cartRepository.UpsertCartAsync(cart);
            return cart;
        }

        // Método auxiliar asíncrono para reutilizar la llamada HTTP a la API de Productos
        private async Task<ProductDto> GetProductFromCatalogAsync(Guid productoId)
        {
            var response = await _productsHttpClient.GetAsync($"api/products/{productoId}");
            if (!response.IsSuccessStatusCode)
                throw new NotFoundException("CRT-002", "Producto no encontrado en el catálogo.");

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ProductDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
    }
}