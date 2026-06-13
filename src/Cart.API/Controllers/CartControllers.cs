namespace Cart.API.Controllers
{
    using Cart.API.Models;
    using Cart.API.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Agrega un ítem al carrito de un usuario.
        /// </summary>
        /// <param name="userId">El Id del usuario dueño del carrito.</param>
        /// <param name="request">Los datos del ítem a agregar.</param>
        [HttpPost("{userId}/items")]
        [ProducesResponseType(typeof(ShoppingCart), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AddItem(Guid userId, [FromBody] CartItem request)
        {
            var cart = await _cartService.AddItemAsync(userId, request);
            return Ok(cart);
        }

        /// <summary>
        /// Obtiene el carrito actual de un usuario con todos sus elementos.
        /// </summary>
        /// <param name="userId">El Id único del usuario.</param>
        /// <response code="200">Devuelve el carrito del usuario.</response>
        /// <response code="404">El carrito no existe o no tiene elementos (ErrorCode: CRT-001).</response>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(ShoppingCart), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCart(Guid userId)
        {
            var cart = await _cartService.GetCartAsync(userId);
            return Ok(cart);
        }

        /// <summary>
        /// Modifica la cantidad establecida de un producto ya existente en el carrito.
        /// </summary>
        /// <param name="userId">El Id del usuario.</param>
        /// <param name="productoId">El Id del producto a modificar.</param>
        /// <param name="cantidad">La nueva cantidad fija del producto.</param>
        /// <response code="200">Cantidad actualizada correctamente.</response>
        /// <response code="404">Carrito o Producto no encontrado (ErrorCode: CRT-001 o CRT-002).</response>
        /// <response code="422">Cantidad inválida o stock insuficiente en catálogo (ErrorCode: CRT-004 o CRT-003).</response>
        [HttpPut("{userId}/items/{productoId}")]
        [ProducesResponseType(typeof(ShoppingCart), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> UpdateItemQuantity(Guid userId, Guid productoId, [FromQuery] int cantidad)
        {
            var cart = await _cartService.UpdateItemQuantityAsync(userId, productoId, cantidad);
            return Ok(cart);
        }

        /// <summary>
        /// Elimina un ítem por completo del carrito del usuario.
        /// </summary>
        /// <param name="userId">El Id del usuario.</param>
        /// <param name="productoId">El Id del producto a remover.</param>
        /// <response code="200">El ítem fue removido y se devuelve el carrito actualizado.</response>
        /// <response code="404">El producto o el carrito no se encontraron (ErrorCode: CRT-001 o CRT-002).</response>
        [HttpDelete("{userId}/items/{productoId}")]
        [ProducesResponseType(typeof(ShoppingCart), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveItem(Guid userId, Guid productoId)
        {
            var cart = await _cartService.RemoveItemAsync(userId, productoId);
            return Ok(cart);
        }
    }
}