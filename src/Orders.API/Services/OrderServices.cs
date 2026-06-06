using Orders.API.Models;
using Orders.API.Exceptions;
using Orders.API.ExceptionHandlers;

namespace Orders.API.Services
{
    public interface IOrderService
    {
        Order GetOrder(Guid id);
        Order CreateOrder(Order request);
    }

    public class OrderService : IOrderService
    {
        private static readonly List<Order> _orders = new();

        public Order GetOrder(Guid id)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order == null) throw new NotFoundException("ORD-001", "Orden no encontrada.");
            return order;
        }

        public Order CreateOrder(Order request)
        {
            if (request.Items == null || !request.Items.Any())
                throw new BadRequestException("ORD-002", "Los datos de la orden son inválidos.");

            decimal totalCalculado = 0;
            foreach (var item in request.Items)
            {
                // TODO: Hacer llamada HTTP real a Products.API para obtener precio y stock
                int stockDisponible = 10; // Simulado

                if (stockDisponible < item.Cantidad)
                    throw new UnprocessableEntityException("ORD-005", $"Stock insuficiente para producto {item.ProductoId}.");

                totalCalculado += (item.PrecioUnitario * item.Cantidad);
            }

            request.Total = totalCalculado;
            request.Estado = "Pendiente";
            request.Id = Guid.NewGuid();
            request.FechaCreacion = DateTime.UtcNow;

            _orders.Add(request);
            return request;
        }
    }
}
