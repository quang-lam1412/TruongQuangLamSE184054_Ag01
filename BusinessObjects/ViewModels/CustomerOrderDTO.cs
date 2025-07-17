using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.ViewModels
{
    public class CustomerOrderDTO
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItemDTO> Items { get; set; } = new(); // Danh sách sản phẩm trong đơn hàng

        public decimal Total => Items.Sum(i => i.Subtotal); // Tổng tiền
        public string ProductNames { get; set; }

    }

    public class OrderItemDTO
    {
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public float Discount { get; set; }

        public decimal Subtotal => UnitPrice * Quantity * (1 - (decimal)Discount);
    }

}
