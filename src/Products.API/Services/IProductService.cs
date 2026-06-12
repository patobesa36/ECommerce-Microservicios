namespace Products.API.Services
{
    using Products.API.Models;
    using Products.API.DTOs;

    public interface IProductService
    {
        List<Product> GetProducts(string? categoria, string? nombre);
        Product GetProduct(Guid id);
        Product CreateProduct(ProductCreateUpdateDto request);
        Product UpdateProduct(Guid id, ProductCreateUpdateDto request);
        void DeleteProduct(Guid id);
    }
}