namespace Cart.API.Controllers
{
    using Cart.API.Models;
    using Cart.API.Services;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        // Inyectamos el servicio por constructor
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost("{userId}/items")]
        // Hacemos el método asíncrono
        public async Task<IActionResult> AddItem(Guid userId, [FromBody] CartItem request)
        {
            // ¡Toda la lógica de validación, HTTP y dominio está encapsulada en el servicio!
            // Si hay un error, el servicio lanzará una Exception y tu IExceptionHandler global lo atrapará.
            var cart = await _cartService.AddItemAsync(userId, request);

            return Ok(cart);
        }

        // Aquí irían el resto de los endpoints (GetCart, UpdateItemQuantity, etc.) delegando también al servicio.
    }
}
