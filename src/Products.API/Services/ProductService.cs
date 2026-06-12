namespace Products.API.Services
{
    using Products.API.Models;
    using Products.API.DTOs;
    using Products.API.Exceptions;
    using Products.API.Data;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ProductService : IProductService
    {
        // 1. Inyectamos la base de datos
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Product>> GetProductsAsync(string? categoria, string? nombre)
        {
            // 2. Buscamos directo en SQLite
            return await _productRepository.GetAllAsync(categoria, nombre);
        }

        public async Task<Product> GetProductAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException("PRD-001", "Producto no encontrado.");

            return product;
        }

        public async Task<Product> CreateProductAsync(ProductCreateUpdateDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) || request.Precio <= 0)
                throw new BusinessRuleException("PRD-002", "Los datos del producto son inválidos.");

            // Validamos si ya existe el nombre en la BD
            var existingProduct = await _productRepository.GetByNameAsync(request.Nombre);
            if (existingProduct != null)
                throw new BusinessRuleException("PRD-003", "Ya existe un producto con el mismo nombre.");

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                Precio = request.Precio,
                Stock = request.Stock,
                Categoria = request.Categoria,
                FechaCreacion = DateTime.UtcNow
            };

            // Guardamos en SQLite
            await _productRepository.CreateAsync(product);
            return product;
        }

        public async Task<Product> UpdateProductAsync(Guid id, ProductCreateUpdateDto request)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException("PRD-001", "Producto no encontrado.");

            if (string.IsNullOrWhiteSpace(request.Nombre) || request.Precio <= 0)
                throw new BusinessRuleException("PRD-002", "Los datos del producto son inválidos.");

            product.Nombre = request.Nombre;
            product.Descripcion = request.Descripcion;
            product.Precio = request.Precio;
            product.Stock = request.Stock;
            product.Categoria = request.Categoria;

            // Actualizamos en SQLite
            await _productRepository.UpdateAsync(product);

            return product;
        }

        public async Task DeleteProductAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException("PRD-001", "Producto no encontrado.");

            // Eliminamos de SQLite
            await _productRepository.DeleteAsync(id);
        }
    }
}
