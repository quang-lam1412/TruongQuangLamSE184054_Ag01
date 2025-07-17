using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;
using BusinessObjects.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BusinessObjects.Services

{
    public class CustomerOrderService : ICustomerOrderService
    {
        private readonly LucySalesDataContext _context;

        public CustomerOrderService(LucySalesDataContext context)
        {
            _context = context;
        }

        public async Task<List<CustomerOrderDTO>> GetOrdersByCustomerIdAsync(int customerId)
        {
            var orders = await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var result = orders.Select(o => new CustomerOrderDTO
            {
                OrderID = o.OrderId,
                OrderDate = o.OrderDate,
                Items = o.OrderDetails.Select(od => new OrderItemDTO
                {
                    ProductName = od.Product.ProductName,
                    UnitPrice = od.UnitPrice,
                    Quantity = od.Quantity,
                    Discount = od.Discount
                }).ToList(),
                ProductNames = string.Join(", ", o.OrderDetails.Select(od => od.Product.ProductName))
            }).ToList();


            return result;
        }
    }   
}
