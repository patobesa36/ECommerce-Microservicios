namespace Products.API.Data;
using Products.API.Models;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync(string? categoria, string? nombre);
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product?> GetByNameAsync(string nombre);
    Task CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Guid id);
}
