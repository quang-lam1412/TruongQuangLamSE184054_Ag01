using BusinessObjects.Models;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using System.Windows;
using ViewModels;
using TruongQuangLamWPF.Windows;
using AppContext = Lucy_SalesData.App;

namespace Lucy_SalesData.Windows
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;
        public Employee? LoggedInEmployee { get; private set; }
        public Customer? LoggedInCustomer { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();

            // Dependency Injection setup
            var serviceProvider = AppContext.ServiceProvider;
            _authService = serviceProvider.GetRequiredService<IAuthService>();

            txtUsername.Focus();

            txtUsername.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                    txtPassword.Focus();
            };

            txtPassword.KeyDown += async (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                    await LoginAsync();
            };
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            await LoginAsync();
        }

        private async Task LoginAsync()
        {
            try
            {
                lblError.Visibility = Visibility.Collapsed;

                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    ShowError("Vui lòng nhập tên đăng nhập hoặc số điện thoại!");
                    txtUsername.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    ShowError("Vui lòng nhập mật khẩu!");
                    txtPassword.Focus();
                    return;
                }

                btnLogin.IsEnabled = false;
                btnLogin.Content = "Đang đăng nhập...";

                // ✅ Tạo LoginViewModel đúng
                var loginModel = new LoginViewModel
                {
                    Identifier = txtUsername.Text.Trim(),
                    Password = txtPassword.Password.Trim(),
                    RememberMe = chkRememberMe.IsChecked ?? false,
                    Role = rdoAdmin.IsChecked == true ? UserRole.Admin : UserRole.Customer

                };

                var result = await _authService.LoginAsync(loginModel);

                if (loginModel.Role == UserRole.Admin && result is Employee emp)
                {
                    LoggedInEmployee = emp;
                    DialogResult = true;
                    Close();
                }
                else if (loginModel.Role == UserRole.Customer && result is Customer cust)
                {
                    LoggedInCustomer = cust;
                    MessageBox.Show("Login thành công! Sẽ mở CustomerWindow");

                    DialogResult = true; // ✅ BẮT BUỘC phải có dòng này để App.xaml.cs biết login thành công

                    Close(); // Close ngay sau khi đặt DialogResult
                }




                else
                {
                    ShowError("Tên đăng nhập hoặc mật khẩu không đúng!");
                    txtPassword.Clear();
                    txtUsername.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi đăng nhập: {ex.Message}");
            }
            finally
            {
                btnLogin.IsEnabled = true;
                btnLogin.Content = "Đăng nhập";
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded && this.IsActive && this.IsVisible)
            {
            
                this.Close();
            }
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visibility = Visibility.Visible;
        }
    }
}
