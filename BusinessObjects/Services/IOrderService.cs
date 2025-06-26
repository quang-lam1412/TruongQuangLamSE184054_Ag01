using BusinessObjects.Models;

namespace Services
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetOrdersByCustomerIdAsync(int customerId);
        Task<List<Order>> GetOrdersByEmployeeIdAsync(int employeeId);
        Task<List<Order>> GetOrdersByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<Order> CreateOrderAsync(Order order);
        Task<bool> UpdateOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(int orderId);
        Task<decimal> GetOrderTotalAsync(int orderId);
        Task<List<OrderDetail>> GetOrderDetailsAsync(int orderId);
        Task<bool> AddOrderDetailAsync(OrderDetail orderDetail);
        Task<bool> UpdateOrderDetailAsync(OrderDetail orderDetail);
        Task<bool> RemoveOrderDetailAsync(int orderId, int productId);
    }
}
