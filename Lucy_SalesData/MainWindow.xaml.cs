using BusinessObjects.Models;
using Microsoft.Extensions.DependencyInjection;
using Services;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;

namespace Lucy_SalesData
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private readonly IDashboardService _dashboardService;

        public MainWindow()
        {
            InitializeComponent();

            // Dependency Injection
            var serviceProvider = ((App)Application.Current).ServiceProvider;
            _dashboardService = serviceProvider.GetRequiredService<IDashboardService>();

            InitializeWindow();
            LoadDashboardData();
            StartClock();
        }

        private void InitializeWindow()
        {
            // Set welcome message
            if (App.CurrentEmployee != null)
            {
                lblWelcome.Text = $"Chào mừng, {App.CurrentEmployee.Name}";
            }
        }

        private async void LoadDashboardData()
        {
            try
            {
                // Hiển thị loading trên UI thread
                Dispatcher.Invoke(() =>
                {
                    lblTotalCustomers.Text = "Đang tải...";
                    lblTotalProducts.Text = "Đang tải...";
                    lblTotalOrders.Text = "Đang tải...";
                    lblTotalRevenue.Text = "Đang tải...";
                });

                // Lấy dữ liệu trên background thread với service scope riêng
                var statistics = await Task.Run(async () =>
                {
                    var serviceProvider = ((App)Application.Current).ServiceProvider;
                    using var scope = serviceProvider.CreateScope();
                    var dashboardService = scope.ServiceProvider.GetRequiredService<IDashboardService>();
                    return await dashboardService.GetDashboardStatisticsAsync();
                });

                // Cập nhật UI trên main thread
                Dispatcher.Invoke(() =>
                {
                    lblTotalCustomers.Text = statistics.TotalCustomers.ToString("N0");
                    lblTotalProducts.Text = statistics.TotalProducts.ToString("N0");
                    lblTotalOrders.Text = statistics.TotalOrders.ToString("N0");

                    // Format tiền tệ Việt Nam
                    var culture = new CultureInfo("vi-VN");
                    lblTotalRevenue.Text = statistics.TotalRevenue.ToString("C0", culture);
                });
            }
            catch (Exception ex)
            {
                // Hiển thị lỗi trên UI thread
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Lỗi tải dữ liệu dashboard: {ex.Message}",
                                  "Lỗi",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);

                    lblTotalCustomers.Text = "Lỗi";
                    lblTotalProducts.Text = "Lỗi";
                    lblTotalOrders.Text = "Lỗi";
                    lblTotalRevenue.Text = "Lỗi";
                });
            }
        }

        private void StartClock()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                lblCurrentTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            };
            _timer.Start();
        }

        private void BtnCustomerManagement_Click(object sender, RoutedEventArgs e)
        {
            var customerWindow = new Windows.CustomerManagementWindow();
            customerWindow.ShowDialog();

            // Refresh sau khi đóng window
            _ = Task.Run(LoadDashboardData);
        }

        private void BtnProductManagement_Click(object sender, RoutedEventArgs e)
        {
            var productWindow = new Windows.ProductManagementWindow();
            productWindow.ShowDialog();

            // Refresh sau khi đóng window
            _ = Task.Run(LoadDashboardData);
        }

        private void BtnOrderProcessing_Click(object sender, RoutedEventArgs e)
        {
            var orderWindow = new Windows.OrderManagementWindow();
            orderWindow.ShowDialog();

            // Refresh sau khi đóng window
            _ = Task.Run(LoadDashboardData);
        }


        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentEmployee != null)
            {
                var employee = App.CurrentEmployee;
                var profileInfo = "THÔNG TIN NHÂN VIÊN\n" + new string('=', 30) + "\n\n";

                profileInfo += $"Mã nhân viên: {employee.EmployeeId}\n";
                profileInfo += $"Tên: {employee.Name}\n";
                profileInfo += $"Tên đăng nhập: {employee.UserName}\n";

                if (!string.IsNullOrEmpty(employee.JobTitle))
                {
                    profileInfo += $"Chức vụ: {employee.JobTitle}\n";
                }

                if (employee.BirthDate.HasValue)
                {
                    profileInfo += $"Ngày sinh: {employee.BirthDate.Value:dd/MM/yyyy}\n";
                }

                if (employee.HireDate.HasValue)
                {
                    profileInfo += $"Ngày vào làm: {employee.HireDate.Value:dd/MM/yyyy}\n";
                }

                if (!string.IsNullOrEmpty(employee.Address))
                {
                    profileInfo += $"Địa chỉ: {employee.Address}\n";
                }

                MessageBox.Show(profileInfo,
                              "Thông tin cá nhân",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
        }

        private void HandleLogout()
        {
            App.CurrentEmployee = null;
            _timer?.Stop();

            // Tạo và hiển thị LoginWindow
            var loginWindow = new Windows.LoginWindow();
            this.Hide(); // Ẩn MainWindow

            var loginResult = loginWindow.ShowDialog();

            if (loginResult == true && loginWindow.LoggedInEmployee != null)
            {
                // Đăng nhập thành công, cập nhật thông tin
                App.CurrentEmployee = loginWindow.LoggedInEmployee;
                InitializeWindow();
                _ = Task.Run(LoadDashboardData);
                StartClock();
                this.Show(); // Hiển thị lại MainWindow
            }
            else
            {
                // Đăng nhập thất bại hoặc hủy, tắt ứng dụng
                Application.Current.Shutdown();
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?",
                                       "Xác nhận đăng xuất",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                HandleLogout();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            base.OnClosed(e);
        }

        // Method để refresh dữ liệu dashboard - SỬA LẠI
        public void RefreshDashboard()
        {
            _ = Task.Run(LoadDashboardData);
        }

        // Thêm method refresh với button
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshDashboard();
        }
    }
}
