namespace Cart.API.Controllers
{
    using Cart.API.Models;
    using Microsoft.AspNetCore.Mvc;
    using Cart.API.Exceptions;

    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        // Simulamos la base de datos
        private static readonly List<Cart_> _carts = new();

        // (Asumimos que tenés acceso a los productos para validar stock)
        // private readonly IProductRepository _productRepo; 

        [HttpGet("{userId}")]
        public IActionResult GetCart(Guid userId)
        {
            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);
            if (cart == null)
                throw new NotFoundException("CRT-001", "Carrito no encontrado."); // 

            return Ok(cart);
        }

        [HttpPost("{userId}/items")]
        public IActionResult AddItem(Guid userId, [FromBody] CartItem request)
        {
            if (request.Cantidad <= 0)
                throw new BusinessRuleException("CRT-004", "Cantidad inválida."); // 

            // Acá en la realidad irías a buscar el producto a la DB
            // var product = _productRepo.GetById(request.ProductoId);
            // if (product == null) throw new NotFoundException("CRT-002", "Producto no encontrado."); 
            // if (product.Stock < request.Cantidad) throw new BusinessRuleException("CRT-003", "Stock insuficiente."); 

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
            return Ok(cart);
        }

        [HttpPut("{userId}/items/{productId}")]
        public IActionResult UpdateItemQuantity(Guid userId, Guid productId, [FromBody] CartItem request)
        {
            if (request.Cantidad <= 0)
                throw new BusinessRuleException("CRT-004", "Cantidad inválida."); // 

            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);
            if (cart == null) throw new NotFoundException("CRT-001", "Carrito no encontrado."); // 

            var item = cart.Items.FirstOrDefault(i => i.ProductoId == productId);
            if (item == null) throw new NotFoundException("CRT-002", "Producto no encontrado en el carrito."); // 

            item.Cantidad = request.Cantidad;
            cart.FechaActualizacion = DateTime.UtcNow;

            return Ok(cart);
        }

        [HttpDelete("{userId}/items/{productId}")]
        public IActionResult RemoveItem(Guid userId, Guid productId)
        {
            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);
            if (cart == null) throw new NotFoundException("CRT-001", "Carrito no encontrado."); // 

            cart.Items.RemoveAll(i => i.ProductoId == productId);
            cart.FechaActualizacion = DateTime.UtcNow;

            return NoContent(); // [cite: 202]
        }

        [HttpDelete("{userId}")]
        public IActionResult ClearCart(Guid userId)
        {
            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);
            if (cart == null) throw new NotFoundException("CRT-001", "Carrito no encontrado."); // 

            _carts.Remove(cart);
            return NoContent(); // [cite: 202]
        }
    }

}
