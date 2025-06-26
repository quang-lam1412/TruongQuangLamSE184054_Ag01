using BusinessObjects.Models;

namespace Repositories
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(int id);
        Task<List<Order>> GetByCustomerIdAsync(int customerId);
        Task<List<Order>> GetByEmployeeIdAsync(int employeeId);
        Task<List<Order>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<Order> AddAsync(Order order);
        Task<bool> UpdateAsync(Order order);
        Task<bool> DeleteAsync(int id);
        Task<List<OrderDetail>> GetOrderDetailsAsync(int orderId);
        Task<bool> AddOrderDetailAsync(OrderDetail orderDetail);
        Task<bool> UpdateOrderDetailAsync(OrderDetail orderDetail);
        Task<bool> RemoveOrderDetailAsync(int orderId, int productId);
    }
}
