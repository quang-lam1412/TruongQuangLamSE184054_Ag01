using BusinessObjects.Models;
using Lucy_SalesData.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Services;
using System.Windows;

namespace Lucy_SalesData.Windows
{
    public partial class OrderDetailsWindow : Window
    {
        private readonly int _orderId;
        private readonly IOrderService _orderService;

        public OrderDetailsWindow(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;

            // Dependency Injection
            var serviceProvider = ((App)Application.Current).ServiceProvider;
            using var scope = serviceProvider.CreateScope();
            _orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

            LoadOrderDetails();
        }

        private async void LoadOrderDetails()
        {
            try
            {
                var serviceProvider = ((App)Application.Current).ServiceProvider;
                using var scope = serviceProvider.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                var order = await orderService.GetOrderByIdAsync(_orderId);

                if (order != null)
                {
                    // Set header
                    lblHeader.Text = $"CHI TIẾT ĐƠN HÀNG #{order.OrderId}";

                    // Set order info
                    lblCustomer.Text = order.Customer?.CompanyName ?? "N/A";
                    lblOrderDate.Text = order.OrderDate.ToString("dd/MM/yyyy");
                    lblEmployee.Text = order.Employee?.Name ?? "N/A";

                    // Calculate total
                    var totalAmount = order.OrderDetails?.Sum(od => od.UnitPrice * od.Quantity * (decimal)(1 - od.Discount)) ?? 0;
                    lblTotalAmount.Text = $"{totalAmount:N0} VNĐ";
                    lblGrandTotal.Text = $"{totalAmount:N0} VNĐ";

                    // Set order details
                    if (order.OrderDetails != null && order.OrderDetails.Any())
                    {
                        var orderDetailViewModels = order.OrderDetails.Select(od => new OrderDetailViewModel
                        {
                            ProductId = od.ProductId,
                            ProductName = od.Product?.ProductName ?? "N/A",
                            UnitPrice = od.UnitPrice,
                            Quantity = od.Quantity,
                            Discount = od.Discount
                        }).ToList();

                        dgOrderDetails.ItemsSource = orderDetailViewModels;
                    }
                }
                else
                {
                    MessageBox.Show("Không tìm thấy đơn hàng!", "Lỗi",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải chi tiết đơn hàng: {ex.Message}", "Lỗi",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
