namespace Cart.API.DTOs
{
    using System;

    public class ProductDto
    {
        public Guid Id { get; set; }
        public int Stock { get; set; }
    }
}