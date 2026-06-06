namespace Orders.API.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid(); 
        public Guid UsuarioId { get; set; } 
        public List<OrderItem> Items { get; set; } = new(); 
        public decimal Total { get; set; } 
        public string Estado { get; set; } = "Pendiente"; 
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow; 
        public DateTime? FechaActualizacion { get; set; }
    }

    public class OrderItem
    {
        public Guid ProductoId { get; set; } 
        public int Cantidad { get; set; } 
        public decimal PrecioUnitario { get; set; } 
    }


}
