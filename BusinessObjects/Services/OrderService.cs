using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;

namespace Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            try
            {
                return await _orderRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy danh sách đơn hàng: {ex.Message}", ex);
            }
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            try
            {
                return await _orderRepository.GetByIdAsync(orderId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy thông tin đơn hàng: {ex.Message}", ex);
            }
        }

        public async Task<List<Order>> GetOrdersByCustomerIdAsync(int customerId)
        {
            try
            {
                return await _orderRepository.GetByCustomerIdAsync(customerId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy đơn hàng theo khách hàng: {ex.Message}", ex);
            }
        }

        public async Task<List<Order>> GetOrdersByEmployeeIdAsync(int employeeId)
        {
            try
            {
                return await _orderRepository.GetByEmployeeIdAsync(employeeId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy đơn hàng theo nhân viên: {ex.Message}", ex);
            }
        }

        public async Task<List<Order>> GetOrdersByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                return await _orderRepository.GetByDateRangeAsync(fromDate, toDate);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy đơn hàng theo ngày: {ex.Message}", ex);
            }
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            try
            {
                // ✅ VALIDATION CƠ BẢN
                if (order == null)
                    throw new ArgumentNullException(nameof(order), "Đơn hàng không được null");

                if (order.CustomerId <= 0)
                    throw new ArgumentException("Khách hàng không hợp lệ");

                if (order.EmployeeId <= 0)
                    throw new ArgumentException("Nhân viên không hợp lệ");

                if (order.OrderDetails == null || !order.OrderDetails.Any())
                    throw new ArgumentException("Đơn hàng phải có ít nhất một sản phẩm");

                // ✅ VALIDATE ORDERDETAILS
                foreach (var detail in order.OrderDetails)
                {
                    if (detail.ProductId <= 0)
                        throw new ArgumentException($"ProductId không hợp lệ: {detail.ProductId}");

                    if (detail.Quantity <= 0)
                        throw new ArgumentException($"Số lượng phải lớn hơn 0. ProductId: {detail.ProductId}");

                    if (detail.UnitPrice <= 0)
                        throw new ArgumentException($"Đơn giá phải lớn hơn 0. ProductId: {detail.ProductId}");

                    if (detail.Discount < 0 || detail.Discount > 1)
                        throw new ArgumentException($"Giảm giá phải từ 0 đến 1. ProductId: {detail.ProductId}");
                }

                // ✅ SET DEFAULT VALUES - KHÔNG SET OrderId
                order.OrderDate = order.OrderDate == default ? DateTime.Now : order.OrderDate;

                // ✅ DEBUG LOG
                System.Diagnostics.Debug.WriteLine($"CreateOrderAsync - CustomerId: {order.CustomerId}, EmployeeId: {order.EmployeeId}");
                System.Diagnostics.Debug.WriteLine($"OrderDate: {order.OrderDate}");
                System.Diagnostics.Debug.WriteLine($"OrderDetails Count: {order.OrderDetails.Count}");

                // ✅ SỬ DỤNG REPOSITORY - Repository sẽ tự tạo OrderId
                var createdOrder = await _orderRepository.AddAsync(order);

                System.Diagnostics.Debug.WriteLine($"Order created successfully with ID: {createdOrder.OrderId}");

                return createdOrder;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateOrderAsync Error: {ex}");
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException?.Message}");

                // Ném lại exception với thông tin chi tiết
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                throw new Exception($"Lỗi tạo đơn hàng: {innerMessage}", ex);
            }
        }


        public async Task<bool> UpdateOrderAsync(Order order)
        {
            try
            {
                return await _orderRepository.UpdateAsync(order);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi cập nhật đơn hàng: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            try
            {
                return await _orderRepository.DeleteAsync(orderId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi xóa đơn hàng: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetOrderTotalAsync(int orderId)
        {
            try
            {
                var orderDetails = await _orderRepository.GetOrderDetailsAsync(orderId);
                return orderDetails.Sum(od => od.UnitPrice * od.Quantity * (decimal)(1 - od.Discount));
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi tính tổng đơn hàng: {ex.Message}", ex);
            }
        }

        public async Task<List<OrderDetail>> GetOrderDetailsAsync(int orderId)
        {
            try
            {
                return await _orderRepository.GetOrderDetailsAsync(orderId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy chi tiết đơn hàng: {ex.Message}", ex);
            }
        }

        public async Task<bool> AddOrderDetailAsync(OrderDetail orderDetail)
        {
            try
            {
                if (orderDetail.Quantity <= 0)
                    throw new ArgumentException("Số lượng phải lớn hơn 0");

                if (orderDetail.UnitPrice <= 0)
                    throw new ArgumentException("Giá phải lớn hơn 0");

                return await _orderRepository.AddOrderDetailAsync(orderDetail);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi thêm sản phẩm vào đơn hàng: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateOrderDetailAsync(OrderDetail orderDetail)
        {
            try
            {
                return await _orderRepository.UpdateOrderDetailAsync(orderDetail);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi cập nhật chi tiết đơn hàng: {ex.Message}", ex);
            }
        }

        public async Task<bool> RemoveOrderDetailAsync(int orderId, int productId)
        {
            try
            {
                return await _orderRepository.RemoveOrderDetailAsync(orderId, productId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi xóa sản phẩm khỏi đơn hàng: {ex.Message}", ex);
            }
        }
    }
}
