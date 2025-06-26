using BusinessObjects.Models;
using Lucy_SalesData.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Services;
using System.Windows;
using System.Windows.Controls;

namespace Lucy_SalesData.Windows
{
    public partial class OrderManagementWindow : Window
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private List<OrderViewModel> _allOrders = new();
        private OrderViewModel? _selectedOrder;

        public OrderManagementWindow()
        {
            InitializeComponent();

            // Dependency Injection
            var serviceProvider = ((App)Application.Current).ServiceProvider;
            using var scope = serviceProvider.CreateScope();
            _orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
            _customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

            InitializeWindow();
            LoadData();
        }

        private void InitializeWindow()
        {
            // Set default date range (last 30 days)
            dpToDate.SelectedDate = DateTime.Now;
            dpFromDate.SelectedDate = DateTime.Now.AddDays(-30);
        }

        private async void LoadData()
        {
            await LoadCustomers();
            await LoadOrders();
        }

        private async Task LoadCustomers()
        {
            try
            {
                var serviceProvider = ((App)Application.Current).ServiceProvider;
                using var scope = serviceProvider.CreateScope();
                var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

                var customers = await customerService.GetAllCustomersAsync();

                Dispatcher.Invoke(() =>
                {
                    cbCustomer.ItemsSource = customers;
                    cbCustomer.SelectedIndex = -1;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Lỗi tải danh sách khách hàng: {ex.Message}",
                                  "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private async Task LoadOrders()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    dgOrders.ItemsSource = null;
                    btnViewDetails.IsEnabled = false;
                    btnEditOrder.IsEnabled = false;
                    btnDeleteOrder.IsEnabled = false;
                });

                var serviceProvider = ((App)Application.Current).ServiceProvider;
                using var scope = serviceProvider.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                List<Order> orders;

                // Apply filters
                var fromDate = dpFromDate.SelectedDate ?? DateTime.Now.AddDays(-30);
                var toDate = dpToDate.SelectedDate ?? DateTime.Now;
                var selectedCustomer = cbCustomer.SelectedValue as int?;

                if (selectedCustomer.HasValue)
                {
                    orders = await orderService.GetOrdersByCustomerIdAsync(selectedCustomer.Value);
                    orders = orders.Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate).ToList();
                }
                else
                {
                    orders = await orderService.GetOrdersByDateRangeAsync(fromDate, toDate);
                }

                // Convert to ViewModels
                var orderViewModels = orders.Select(o => new OrderViewModel
                {
                    OrderId = o.OrderId,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer?.CompanyName ?? "N/A",
                    EmployeeId = o.EmployeeId,
                    EmployeeName = o.Employee?.Name ?? "N/A",
                    OrderDate = o.OrderDate,
                    TotalAmount = o.OrderDetails?.Sum(od => od.UnitPrice * od.Quantity * (decimal)(1 - od.Discount)) ?? 0,
                    TotalItems = o.OrderDetails?.Count ?? 0
                }).ToList();

                _allOrders = orderViewModels;

                Dispatcher.Invoke(() =>
                {
                    dgOrders.ItemsSource = _allOrders;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Lỗi tải danh sách đơn hàng: {ex.Message}",
                                  "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private void DgOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedOrder = dgOrders.SelectedItem as OrderViewModel;
            bool hasSelection = _selectedOrder != null;

            btnViewDetails.IsEnabled = hasSelection;
            btnEditOrder.IsEnabled = hasSelection;
            btnDeleteOrder.IsEnabled = hasSelection;
        }

        private async void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            await LoadOrders();
        }

        private void BtnCreateOrder_Click(object sender, RoutedEventArgs e)
        {
            var createOrderWindow = new CreateOrderWindow();
            var result = createOrderWindow.ShowDialog();

            if (result == true)
            {
                _ = Task.Run(LoadOrders);
            }
        }

        private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder != null)
            {
                var detailsWindow = new OrderDetailsWindow(_selectedOrder.OrderId);
                detailsWindow.ShowDialog();
            }
        }

        private void BtnEditOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder != null)
            {
                var editWindow = new EditOrderWindow(_selectedOrder.OrderId);
                var result = editWindow.ShowDialog();

                if (result == true)
                {
                    _ = Task.Run(LoadOrders);
                }
            }
        }


        private async void BtnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder != null)
            {
                var result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa đơn hàng #{_selectedOrder.OrderId}?\n" +
                    $"Khách hàng: {_selectedOrder.CustomerName}\n" +
                    $"Tổng tiền: {_selectedOrder.TotalAmount:N0} VNĐ",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var serviceProvider = ((App)Application.Current).ServiceProvider;
                        using var scope = serviceProvider.CreateScope();
                        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                        var success = await orderService.DeleteOrderAsync(_selectedOrder.OrderId);

                        if (success)
                        {
                            MessageBox.Show("Xóa đơn hàng thành công!", "Thành công",
                                          MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadOrders();
                        }
                        else
                        {
                            MessageBox.Show("Không thể xóa đơn hàng!", "Lỗi",
                                          MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi xóa đơn hàng: {ex.Message}", "Lỗi",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadOrders();
        }
    }
}
