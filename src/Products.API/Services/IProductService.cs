namespace Products.API.Services
{
    using Products.API.Models;
    using Products.API.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IProductService
    {
        Task<IEnumerable<Product>> GetProductsAsync(string? categoria, string? nombre);
        Task<Product> GetProductAsync(Guid id);
        Task<Product> CreateProductAsync(ProductCreateUpdateDto request);
        Task<Product> UpdateProductAsync(Guid id, ProductCreateUpdateDto request);
        Task DeleteProductAsync(Guid id);
    }
}