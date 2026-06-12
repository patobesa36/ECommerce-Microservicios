namespace Orders.API.Services
{
    using Orders.API.Models;
    using Orders.API.Exceptions;
    using Orders.API.DTOs;
    using Orders.API.Data;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class OrderService : IOrderServices
    {
        // 1. Inyectamos nuestro repositorio y nuestros clientes HTTP
        private readonly IOrderRepository _orderRepository;
        private readonly HttpClient _productsHttpClient;
        private readonly IUsersApiClient _usersApiClient;

        public OrderService(
            IOrderRepository orderRepository,
            HttpClient productsHttpClient,
            IUsersApiClient usersApiClient)
        {
            _orderRepository = orderRepository;
            _usersApiClient = usersApiClient;

            _productsHttpClient = productsHttpClient;
            _productsHttpClient.BaseAddress = new Uri("https://localhost:7001/"); // Puerto de Products.API
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync(Guid userId)
        {
            // Buscamos directo en SQLite
            return await _orderRepository.GetByUserIdAsync(userId);
        }

        public async Task<Order> GetOrderAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new NotFoundException("ORD-001", "La orden solicitada no existe.");

            return order;
        }

        public async Task<Order> CreateOrderAsync(Order request)
        {
            if (request.Items == null || !request.Items.Any())
                throw new BusinessRuleException("ORD-002", "Los datos de la orden son inválidos.");

            // Validamos que el usuario exista en Users.API
            var userExists = await _usersApiClient.UserExistsAsync(request.UsuarioId);
            if (!userExists)
                throw new NotFoundException("ORD-003", $"El usuario con ID {request.UsuarioId} no existe o no es válido.");

            decimal totalCalculado = 0;

            foreach (var item in request.Items)
            {
                var response = await _productsHttpClient.GetAsync($"api/products/{item.ProductoId}");

                if (!response.IsSuccessStatusCode)
                    throw new NotFoundException("ORD-004", "Producto no encontrado al crear la orden.");

                var productContent = await response.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<ProductDto>(productContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (product == null || product.Stock < item.Cantidad)
                    throw new BusinessRuleException("ORD-005", $"Stock insuficiente. Disponible: {product?.Stock}, solicitado: {item.Cantidad}.");

                item.PrecioUnitario = product.Precio;
                totalCalculado += (item.PrecioUnitario * item.Cantidad);
            }

            request.Total = totalCalculado;
            request.Estado = "Pendiente";
            request.Id = Guid.NewGuid();
            request.FechaCreacion = DateTime.UtcNow;

            // 2. Guardamos la cabecera y el detalle en SQLite usando nuestra Transacción
            await _orderRepository.CreateAsync(request);

            return request;
        }

        public async Task<Order> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusDto request)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new NotFoundException("ORD-001", "La orden solicitada no existe.");

            // 3. Actualizamos el estado en SQLite
            await _orderRepository.UpdateStatusAsync(id, request.Estado);

            order.Estado = request.Estado;
            return order;
        }
    }
}
