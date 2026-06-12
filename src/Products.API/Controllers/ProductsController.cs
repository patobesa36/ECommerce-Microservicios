namespace Products.API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Products.API.DTOs;
    using Products.API.Models;
    using Products.API.Services;

    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Lista los productos disponibles, opcionalmente filtrados por categoría o nombre.
        /// </summary>
        /// <param name="categoria">Filtro opcional por categoría (ej. Electrónica)</param>
        /// <param name="nombre">Filtro opcional por nombre</param>
        /// <response code="200">Retorna la lista de productos</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult GetProducts([FromQuery] string? categoria, [FromQuery] string? nombre)
        {
            return Ok(_productService.GetProducts(categoria, nombre));
        }

        /// <summary>
        /// Obtiene el detalle de un producto específico por su ID.
        /// </summary>
        /// <response code="200">Retorna el producto solicitado</response>
        /// <response code="404">Producto no encontrado (ErrorCode: PRD-001)</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public IActionResult GetProduct(Guid id)
        {
            return Ok(_productService.GetProduct(id));
        }

        /// <summary>
        /// Crea un nuevo producto en el catálogo.
        /// </summary>
        /// <response code="201">Producto creado exitosamente</response>
        /// <response code="422">Los datos del producto son inválidos (ErrorCode: PRD-002) o ya existe (ErrorCode: PRD-003)</response>
        [HttpPost]
        [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public IActionResult CreateProduct([FromBody] ProductCreateUpdateDto request)
        {
            var createdProduct = _productService.CreateProduct(request);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }

        /// <summary>
        /// Actualiza los datos de un producto existente.
        /// </summary>
        /// <response code="200">Producto actualizado exitosamente</response>
        /// <response code="404">Producto no encontrado (ErrorCode: PRD-001)</response>
        /// <response code="422">Los datos del producto son inválidos (ErrorCode: PRD-002)</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
     
        public IActionResult UpdateProduct(Guid id, [FromBody] ProductCreateUpdateDto request)
        {
            return Ok(_productService.UpdateProduct(id, request));
        }

        /// <summary>
        /// Elimina un producto del catálogo.
        /// </summary>
        /// <response code="204">Producto eliminado correctamente</response>
        /// <response code="404">Producto no encontrado (ErrorCode: PRD-001)</response>
        /// <response code="422">El producto tiene órdenes activas y no puede eliminarse (ErrorCode: PRD-004)</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public IActionResult DeleteProduct(Guid id)
        {
            _productService.DeleteProduct(id);
            return NoContent();
        }
    }
}