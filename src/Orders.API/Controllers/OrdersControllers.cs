namespace Orders.API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Orders.API.Models;

    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private static readonly List<Order> _orders = new();

        [HttpGet]
        public IActionResult GetOrders([FromQuery] Guid? usuarioId)
        {
            var query = _orders.AsQueryable();
            if (usuarioId.HasValue) query = query.Where(o => o.UsuarioId == usuarioId);

            return Ok(query.ToList());
        }

        [HttpGet("{id}")]
        public IActionResult GetOrder(Guid id)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order == null) throw new NotFoundException("ORD-001", "Orden no encontrada."); // [cite: 154]

            return Ok(order);
        }

        [HttpPost]
        public IActionResult CreateOrder([FromBody] Order request)
        {
            if (request.Items == null || !request.Items.Any())
                throw new BusinessRuleException("ORD-002", "Los datos de la orden son inválidos."); // 

            // Lógica de cálculo: en un entorno real, buscarías el precio de cada producto en la BD.
            decimal totalCalculado = 0;
            foreach (var item in request.Items)
            {
                // Validar stock real (simulado acá)
                [cite_start]// if (producto.Stock < item.Cantidad) throw new BusinessRuleException("ORD-005", "Stock insuficiente..."); 

                totalCalculado += (item.PrecioUnitario * item.Cantidad);
            }

            request.Total = totalCalculado;
            request.Estado = "Pendiente";
            request.Id = Guid.NewGuid();
            request.FechaCreacion = DateTime.UtcNow;

            _orders.Add(request);
            return CreatedAtAction(nameof(GetOrder), new { id = request.Id }, request); // [cite: 152]
        }

        [HttpPut("{id}/status")]
        public IActionResult UpdateOrderStatus(Guid id, [FromBody] Order statusUpdate)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order == null) throw new NotFoundException("ORD-001", "Orden no encontrada."); // [cite: 154]

            // Validación de máquina de estados: No podés volver de Entregada a Pendiente
            if (order.Estado == "Entregada" && statusUpdate.Estado == "Pendiente")
                throw new BusinessRuleException("ORD-006", "El estado de la orden no puede ser modificado."); // 

            order.Estado = statusUpdate.Estado;
            order.FechaActualizacion = DateTime.UtcNow;

            return Ok(order);
        }
    }

}
