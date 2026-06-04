namespace Orders.API.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Identificador único [cite: 350]
        public Guid UsuarioId { get; set; } // Referencia al usuario [cite: 350]
        public List<OrderItem> Items { get; set; } = new(); // Lista de productos [cite: 350]
        public decimal Total { get; set; } // Calculado automáticamente [cite: 350]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Confirmada, Enviada, etc. [cite: 350]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow; // Asignado automáticamente [cite: 350]
        public DateTime? FechaActualizacion { get; set; }
    }

    public class OrderItem
    {
        public Guid ProductoId { get; set; } // Referencia al producto [cite: 352, 354]
        public int Cantidad { get; set; } // Requerido, mayor a 0 [cite: 355, 356]
        public decimal PrecioUnitario { get; set; } // Capturado del producto al crear la orden [cite: 356, 358]
    }


}
