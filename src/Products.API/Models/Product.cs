namespace Products.API.Models;

public class Product
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public required string Nombre { get; set; }
  public string? Descripcion { get; set; }
  public decimal Precio { get; set; }
  public int Stock { get; set; }
  public required string Categoria { get; set; }
  public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}