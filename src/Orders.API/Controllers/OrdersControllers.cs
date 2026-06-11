namespace Orders.API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Orders.API.Models;
    using Orders.API.Services;

    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        // Inyectamos el servicio
        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public IActionResult GetOrders([FromQuery] Guid? usuarioId)
        {
            return Ok(_orderService.GetOrders(usuarioId));
        }

        [HttpGet("{id}")]
        public IActionResult GetOrder(Guid id)
        {
            return Ok(_orderService.GetOrder(id));
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order request)
        {
            var order = await _orderService.CreateOrderAsync(request);
            // Mantenemos el CreatedAtAction que ya tenías para devolver código 201 [cite: 1171]
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        [HttpPut("{id}/status")]
        public IActionResult UpdateOrderStatus(Guid id, [FromBody] Order statusUpdate)
        {
            var order = _orderService.UpdateOrderStatus(id, statusUpdate);
            return Ok(order);
        }
    }
}
