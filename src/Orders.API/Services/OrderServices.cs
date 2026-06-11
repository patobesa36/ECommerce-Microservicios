namespace Orders.API.Services
{
    using Orders.API.Models;
    using Orders.API.Exceptions;
    using System.Text.Json;

    public class OrderService : IOrderService
    {
        // Mudamos la base de datos simulada acá
        private static readonly List<Order> _orders = new();

        private readonly HttpClient _httpClient;

        public OrderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            // IMPORTANTE: Asegurate de que este puerto sea el de tu Products.API
            _httpClient.BaseAddress = new Uri("https://localhost:7001/");
        }

        public List<Order> GetOrders(Guid? usuarioId)
        {
            var query = _orders.AsQueryable();
            if (usuarioId.HasValue) query = query.Where(o => o.UsuarioId == usuarioId);
            return query.ToList();
        }

        public Order GetOrder(Guid id)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order == null) throw new NotFoundException("ORD-001", "Orden no encontrada."); // [cite: 1164]
            return order;
        }

        public async Task<Order> CreateOrderAsync(Order request)
        {
            if (request.Items == null || !request.Items.Any())
                throw new BusinessRuleException("ORD-002", "Los datos de la orden son inválidos."); // [cite: 1165]

            // Falta validar el usuario (ORD-003) en Users API, lo podés agregar después.

            decimal totalCalculado = 0;

            foreach (var item in request.Items)
            {
                // 1. Buscamos el producto en la API real
                var response = await _httpClient.GetAsync($"api/products/{item.ProductoId}");

                if (!response.IsSuccessStatusCode)
                    throw new NotFoundException("ORD-004", "Producto no encontrado al crear la orden."); // [cite: 1167]

                var productContent = await response.Content.ReadAsStringAsync();
                var product = JsonSerializer.Deserialize<ProductDto>(productContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // 2. Validamos el stock real que exige el TP
                if (product == null || product.Stock < item.Cantidad)
                    throw new BusinessRuleException("ORD-005", $"Stock insuficiente. Disponible: {product?.Stock}, solicitado: {item.Cantidad}."); // [cite: 1168]

                // 3. Tomamos el precio real del producto (no confiamos en el precio que envía el front-end/Swagger)
                item.PrecioUnitario = product.Precio;
                totalCalculado += (item.PrecioUnitario * item.Cantidad);
            }

            request.Total = totalCalculado;
            request.Estado = "Pendiente";
            request.Id = Guid.NewGuid();
            request.FechaCreacion = DateTime.UtcNow;

            _orders.Add(request);
            return request;
        }

        public Order UpdateOrderStatus(Guid id, Order statusUpdate)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order == null) throw new NotFoundException("ORD-001", "Orden no encontrada."); // [cite: 1164]

            if (order.Estado == "Entregada" && statusUpdate.Estado == "Pendiente")
                throw new BusinessRuleException("ORD-006", "El estado de la orden no puede ser modificado."); // [cite: 1169]

            order.Estado = statusUpdate.Estado;
            order.FechaActualizacion = DateTime.UtcNow;

            return order;
        }

        // Clase auxiliar para mapear el JSON de la API de Productos
        private class ProductDto
        {
            public Guid Id { get; set; }
            public int Stock { get; set; }
            public decimal Precio { get; set; }
        }
    }
}
