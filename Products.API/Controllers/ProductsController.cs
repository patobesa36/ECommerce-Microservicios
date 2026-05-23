using Microsoft.AspNetCore.Mvc;
using Products.API.Models;
using Products.API.Exceptions;

namespace Products.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
  private static readonly List<Product> _products = new();

  [HttpGet]
  public IActionResult GetProducts([FromQuery] string? categoria, [FromQuery] string? nombre)
  {
    var query = _products.AsQueryable();
    if (!string.IsNullOrEmpty(categoria)) query = query.Where(p => p.Categoria == categoria);
    if (!string.IsNullOrEmpty(nombre)) query = query.Where(p => p.Nombre.Contains(nombre));

    return Ok(query.ToList());
  }

  [HttpGet("{id}")]
  public IActionResult GetProduct(Guid id)
  {
    var product = _products.FirstOrDefault(p => p.Id == id);
    if (product == null)
      throw new NotFoundException("PRD-001", "Producto no encontrado.");

    return Ok(product);
  }

  [HttpPost]
  public IActionResult CreateProduct([FromBody] Product request)
  {
    if (_products.Any(p => p.Nombre == request.Nombre && p.Categoria == request.Categoria))
      throw new BusinessRuleException("PRD-003", $"Ya existe un producto con ese nombre en la categoria '{request.Categoria}'.");

    _products.Add(request);
    return CreatedAtAction(nameof(GetProduct), new { id = request.Id }, request);
  }

  [HttpPut("{id}")]
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

  [HttpDelete("{id}")]
  public IActionResult DeleteProduct(Guid id)
  {
    var product = _products.FirstOrDefault(p => p.Id == id);
    if (product == null)
      throw new NotFoundException("PRD-001", "Producto no encontrado.");

    _products.Remove(product);
    return NoContent();
  }
}