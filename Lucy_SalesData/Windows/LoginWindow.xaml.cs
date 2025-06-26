using BusinessObjects.Models;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using System.Windows;
using ViewModels;

namespace Lucy_SalesData.Windows
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;
        public Employee? LoggedInEmployee { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();

            // Dependency Injection setup
            var serviceProvider = ((App)Application.Current).ServiceProvider;
            _authService = serviceProvider.GetRequiredService<IAuthService>();

            // Set focus to username textbox
            txtUsername.Focus();

            // Handle Enter key press
            txtUsername.KeyDown += (s, e) => { if (e.Key == System.Windows.Input.Key.Enter) txtPassword.Focus(); };
            txtPassword.KeyDown += async (s, e) => { if (e.Key == System.Windows.Input.Key.Enter) await LoginAsync(); };
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            await LoginAsync();
        }

        private async Task LoginAsync()
        {
            try
            {
                // Hide error message
                lblError.Visibility = Visibility.Collapsed;

                // Validate input
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    ShowError("Vui lòng nhập tên đăng nhập!");
                    txtUsername.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    ShowError("Vui lòng nhập mật khẩu!");
                    txtPassword.Focus();
                    return;
                }

                // Disable login button during authentication
                btnLogin.IsEnabled = false;
                btnLogin.Content = "Đang đăng nhập...";

                // Create login model
                var loginModel = new LoginViewModel
                {
                    UserName = txtUsername.Text.Trim(),
                    Password = txtPassword.Password.Trim(),
                    RememberMe = chkRememberMe.IsChecked ?? false
                };

                // Authenticate user
                LoggedInEmployee = await _authService.LoginAsync(loginModel);

                if (LoggedInEmployee != null)
                {
                    // Login successful
                    DialogResult = true;
                    Close();
                }
                else
                {
                    // Login failed
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
                // Re-enable login button
                btnLogin.IsEnabled = true;
                btnLogin.Content = "Đăng nhập";
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visibility = Visibility.Visible;
        }
    }
}
