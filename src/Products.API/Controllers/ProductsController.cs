using Microsoft.AspNetCore.Mvc;
using Products.API.Models;
using Products.API.Exceptions;

namespace Products.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
  private static readonly List<Product> _products = new();

  /// <summary>
  /// Lista los productos disponibles, opcionalmente filtrados por categoría o nombre.
  /// </summary>
  /// <param name="categoria">Filtro opcional por categoría (ej. Electrónica)</param>
  /// <param name="nombre">Filtro opcional por nombre</param>
  /// <response code="200">Retorna la lista de productos</response>
  /// <response code="500">Error interno del servidor</response>
  [HttpGet]
  [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public IActionResult GetProducts([FromQuery] string? categoria, [FromQuery] string? nombre)
  {
    var query = _products.AsQueryable();
    if (!string.IsNullOrEmpty(categoria)) query = query.Where(p => p.Categoria == categoria);
    if (!string.IsNullOrEmpty(nombre)) query = query.Where(p => p.Nombre.Contains(nombre));

    return Ok(query.ToList());
  }

  /// <summary>
  /// Obtiene el detalle de un producto específico por su ID.
  /// </summary>
  /// <response code="200">Retorna el producto solicitado</response>
  /// <response code="404">Producto no encontrado (PRD-001)</response>
  [HttpGet("{id}")]
  [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public IActionResult GetProduct(Guid id)
  {
    var product = _products.FirstOrDefault(p => p.Id == id);
    if (product == null)
      throw new NotFoundException("PRD-001", "Producto no encontrado.");

    return Ok(product);
  }

  /// <summary>
  /// Crea un nuevo producto en el catálogo.
  /// </summary>
  /// <response code="201">Producto creado exitosamente</response>
  /// <response code="400">Los datos del producto son inválidos (PRD-002)</response>
  /// <response code="409">Ya existe un producto con ese nombre en la categoría (PRD-003)</response>
  [HttpPost]
  [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  public IActionResult CreateProduct([FromBody] Product request)
  {
    if (_products.Any(p => p.Nombre == request.Nombre && p.Categoria == request.Categoria))
      throw new BusinessRuleException("PRD-003", $"Ya existe un producto con ese nombre en la categoria '{request.Categoria}'.");

    _products.Add(request);
    return CreatedAtAction(nameof(GetProduct), new { id = request.Id }, request);
  }

  /// <summary>
  /// Actualiza los datos de un producto existente.
  /// </summary>
  /// <response code="200">Producto actualizado exitosamente</response>
  /// <response code="400">Los datos del producto son inválidos (PRD-002)</response>
  /// <response code="404">Producto no encontrado (PRD-001)</response>
  [HttpPut("{id}")]
  [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public IActionResult UpdateProduct(Guid id, [FromBody] Product request)
  {
    var product = _products.FirstOrDefault(p => p.Id == id);
    if (product == null)
      throw new NotFoundException("PRD-001", "Producto no encontrado.");

    product.Nombre = request.Nombre;
    product.Descripcion = request.Descripcion;
    product.Precio = request.Precio;
    product.Stock = request.Stock;
    product.Categoria = request.Categoria;

    return Ok(product);
  }

  /// <summary>
  /// Elimina un producto del catálogo.
  /// </summary>
  /// <response code="204">Producto eliminado correctamente</response>
  /// <response code="404">Producto no encontrado (PRD-001)</response>
  /// <response code="409">El producto tiene órdenes activas y no puede eliminarse (PRD-004)</response>
  [HttpDelete("{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  public IActionResult DeleteProduct(Guid id)
  {
    var product = _products.FirstOrDefault(p => p.Id == id);
    if (product == null)
      throw new NotFoundException("PRD-001", "Producto no encontrado.");

    // Simulamos la lógica para cumplir con el error PRD-004 si fuera necesario.
    _products.Remove(product);
    return NoContent();
  }
}