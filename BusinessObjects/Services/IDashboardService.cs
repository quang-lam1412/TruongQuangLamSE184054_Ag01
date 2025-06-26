using BusinessObjects.Models;

namespace Services
{
    public interface IDashboardService
    {
        Task<DashboardStatistics> GetDashboardStatisticsAsync();
    }

    public class DashboardStatistics
    {
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
