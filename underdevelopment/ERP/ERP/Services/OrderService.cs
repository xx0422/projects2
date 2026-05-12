using ERP.Data;
using ERP.Models;
using Microsoft.EntityFrameworkCore;

namespace ERP.Services
{
    public class OrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }
     
        public async Task<Order> CreateOrderFromInvoiceAsync(Invoice invoice)
        {
            if (invoice == null) throw new ArgumentNullException(nameof(invoice));

            var order = new Order
            {
                OrderDate = DateTime.Now,
                Status = OrderStatus.Processing, 
                CustomerId = 0, 
                CustomerName = invoice.CustomerName,
                Items = new List<OrderItem>()
            };

            foreach (var invItem in invoice.Items)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = invItem.ProductId,
                    Quantity = invItem.Quantity,
                    UnitPriceAtTimeOfOrder = invItem.UnitPrice
                });
            }

            _context.Orders.Add(order);
            // Itt NEM hívunk SaveChangesAsync-ot, mert az InvoiceService tranzakciója fogja elmenteni!
            return order;
        }
    }
}