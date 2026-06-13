namespace Orders.API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Orders.API.Models;
    using Orders.API.Services;
    using Orders.API.DTOs; // Agregado para que reconozca UpdateOrderStatusDto
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderServices _orderService;

        public OrdersController(IOrderServices orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Obtiene todas las órdenes, opcionalmente filtradas por usuario.
        /// </summary>
        /// <param name="usuarioId">El Id del usuario para filtrar sus órdenes (Opcional).</param>
        /// <returns>Una lista de órdenes.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<Order>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrders([FromQuery] Guid? usuarioId)
        {
            // Usamos await y llamamos al método Async definido en la interfaz
            var orders = await _orderService.GetOrdersAsync(usuarioId ?? Guid.Empty);
            return Ok(orders);
        }

        /// <summary>
        /// Obtiene una orden específica por su Id.
        /// </summary>
        /// <param name="id">El Id único de la orden.</param>
        /// <returns>La orden solicitada.</returns>
        /// <response code="200">Devuelve la orden correctamente.</response>
        /// <response code="404">Orden no encontrada (ErrorCode: ORD-001).</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            // Usamos await y llamamos al método Async definido en la interfaz
            var order = await _orderService.GetOrderAsync(id);
            return Ok(order);
        }

        /// <summary>
        /// Crea una nueva orden de compra.
        /// </summary>
        /// <remarks>
        /// Valida el stock real contra el microservicio de Productos. El precio final se calcula automáticamente en el servidor.
        /// </remarks>
        /// <param name="request">Los datos de la orden a crear.</param>
        /// <returns>La orden recién creada.</returns>
        /// <response code="201">La orden fue creada exitosamente.</response>
        /// <response code="422">Los datos son inválidos (ErrorCode: ORD-002) o el Stock es insuficiente (ErrorCode: ORD-005).</response>
        /// <response code="404">Algún producto solicitado no existe (ErrorCode: ORD-004).</response>
        [HttpPost]
        [ProducesResponseType(typeof(Order), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateOrder([FromBody] Order request)
        {
            // Este método ya lo tenías bien
            var order = await _orderService.CreateOrderAsync(request);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        /// <summary>
        /// Actualiza el estado de una orden.
        /// </summary>
        /// <param name="id">El Id de la orden a actualizar.</param>
        /// <param name="statusUpdate">El objeto con el nuevo estado (ej. "Entregada").</param>
        /// <returns>La orden actualizada.</returns>
        /// <response code="200">El estado se actualizó correctamente.</response>
        /// <response code="404">Orden no encontrada (ErrorCode: ORD-001).</response>
        /// <response code="422">Regla de negocio violada, ej. pasar de Entregada a Pendiente (ErrorCode: ORD-006).</response>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusDto statusUpdate)
        {
            // Usamos await y llamamos al método Async pasando el DTO correcto
            var order = await _orderService.UpdateOrderStatusAsync(id, statusUpdate);
            return Ok(order);
        }
    }
}