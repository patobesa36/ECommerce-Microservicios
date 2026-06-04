namespace Cart.API.Models
{
    public class Cart
    {
        public Guid UsuarioId { get; set; } // Identificador del usuario dueño del carrito [cite: 360, 364]
        public List<CartItem> Items { get; set; } = new(); // Lista de productos en el carrito [cite: 361, 365]
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow; // Actualizado automáticamente [cite: 362, 366]
    }

    public class CartItem
    {
        public Guid ProductoId { get; set; } // Referencia al producto [cite: 368]
        public int Cantidad { get; set; } // Requerido, mayor a 0 [cite: 369]
    }

}
