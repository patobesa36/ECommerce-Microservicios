namespace Cart.API.Models
{
    public class Cart_
    {
        public Guid UsuarioId { get; set; } 
        public List<CartItem> Items { get; set; } = new(); 
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow; 
    }

    public class CartItem
    {
        public Guid ProductoId { get; set; } 
        public int Cantidad { get; set; } 
    }


}
