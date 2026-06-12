namespace Products.API.Services
{
    using Products.API.DTOs;
    using Products.API.Exceptions;
    using Products.API.Models;

    public class ProductService : IProductService
    {
       
        private static readonly List<Product> _products = new();

        public List<Product> GetProducts(string? categoria, string? nombre)
        {
            var query = _products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(p => p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(p => p.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase));

            return query.ToList();
        }

        public Product GetProduct(Guid id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                throw new NotFoundException("PRD-001", "Producto no encontrado.");

            return product;
        }

        public Product CreateProduct(ProductCreateUpdateDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) || request.Precio <= 0)
                throw new BusinessRuleException("PRD-002", "Los datos del producto son inválidos.");

            if (_products.Any(p => p.Nombre.Equals(request.Nombre, StringComparison.OrdinalIgnoreCase)))
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

            _products.Add(product);
            return product;
        }

        public Product UpdateProduct(Guid id, ProductCreateUpdateDto request)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                throw new NotFoundException("PRD-001", "Producto no encontrado.");

            if (string.IsNullOrWhiteSpace(request.Nombre) || request.Precio <= 0)
                throw new BusinessRuleException("PRD-002", "Los datos del producto son inválidos.");

            
            product.Nombre = request.Nombre;
            product.Descripcion = request.Descripcion;
            product.Precio = request.Precio;
            product.Stock = request.Stock;
            product.Categoria = request.Categoria;

            return product;
        }

        public void DeleteProduct(Guid id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                throw new NotFoundException("PRD-001", "Producto no encontrado.");

            _products.Remove(product);
        }
    }
}
