using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class DashboardService : IDashboardService
    {
        private readonly LucySalesDataContext _context;

        public DashboardService(LucySalesDataContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatistics> GetDashboardStatisticsAsync()
        {
            try
            {
                var statistics = new DashboardStatistics();

                // Đếm tổng số khách hàng
                statistics.TotalCustomers = await _context.Customers
                    .CountAsync()
                    .ConfigureAwait(false);

                // Đếm tổng số sản phẩm (không bao gồm sản phẩm đã ngừng kinh doanh)
                statistics.TotalProducts = await _context.Products
                    .Where(p => !p.Discontinued)
                    .CountAsync()
                    .ConfigureAwait(false);

                // Đếm tổng số đơn hàng
                statistics.TotalOrders = await _context.Orders
                    .CountAsync()
                    .ConfigureAwait(false);

                // Tính tổng doanh thu từ tất cả đơn hàng
                statistics.TotalRevenue = await _context.OrderDetails
                    .SumAsync(od => od.UnitPrice * od.Quantity * (decimal)(1 - od.Discount))
                    .ConfigureAwait(false);

                return statistics;
            }
            catch (Exception ex)
            {
                // Log error nếu cần
                throw new Exception($"Lỗi khi tải dữ liệu dashboard: {ex.Message}", ex);
            }
        }
    }
}
