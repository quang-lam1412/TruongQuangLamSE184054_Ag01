using BusinessObjects.Models;
using Microsoft.Extensions.DependencyInjection;
using Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lucy_SalesData.Windows
{
    public partial class CustomerManagementWindow : Window
    {
        private readonly ICustomerService _customerService;
        private ObservableCollection<Customer> _customers;
        private List<Customer> _allCustomers;

        public CustomerManagementWindow()
        {
            InitializeComponent();

            // Dependency Injection
            var serviceProvider = ((App)Application.Current).ServiceProvider;
            _customerService = serviceProvider.GetRequiredService<ICustomerService>();

            _customers = new ObservableCollection<Customer>();
            dgCustomers.ItemsSource = _customers;

            LoadCustomers();
        }

        private async void LoadCustomers()
        {
            try
            {
                lblStatus.Text = "Đang tải danh sách khách hàng...";

                _allCustomers = await _customerService.GetAllCustomersAsync();

                _customers.Clear();
                foreach (var customer in _allCustomers)
                {
                    _customers.Add(customer);
                }

                UpdateStatusBar();
                lblStatus.Text = "Tải danh sách khách hàng thành công";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách khách hàng: {ex.Message}",
                              "Lỗi",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                lblStatus.Text = "Lỗi tải dữ liệu";
            }
        }

        private void UpdateStatusBar()
        {
            lblTotalRecords.Text = $"Tổng: {_customers.Count} khách hàng";
        }

        private void DgCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = dgCustomers.SelectedItem != null;
            btnEdit.IsEnabled = hasSelection;
            btnDelete.IsEnabled = hasSelection;
        }

        private void DgCustomers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgCustomers.SelectedItem is Customer customer)
            {
                EditCustomer(customer);
            }
        }

        private async void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            await PerformSearch();
        }

        private async void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await PerformSearch();
            }
        }

        private async Task PerformSearch()
        {
            try
            {
                string searchTerm = txtSearch.Text.Trim();

                if (string.IsNullOrEmpty(searchTerm))
                {
                    // Show all customers
                    _customers.Clear();
                    foreach (var customer in _allCustomers)
                    {
                        _customers.Add(customer);
                    }
                }
                else
                {
                    lblStatus.Text = "Đang tìm kiếm...";
                    var searchResults = await _customerService.SearchCustomersAsync(searchTerm);

                    _customers.Clear();
                    foreach (var customer in searchResults)
                    {
                        _customers.Add(customer);
                    }

                    lblStatus.Text = $"Tìm thấy {searchResults.Count} kết quả";
                }

                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm kiếm: {ex.Message}",
                              "Lỗi",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                lblStatus.Text = "Lỗi tìm kiếm";
            }
        }

        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var addEditWindow = new CustomerAddEditWindow();
            if (addEditWindow.ShowDialog() == true)
            {
                LoadCustomers(); // Refresh the list
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgCustomers.SelectedItem is Customer customer)
            {
                EditCustomer(customer);
            }
        }

        private void EditCustomer(Customer customer)
        {
            var addEditWindow = new CustomerAddEditWindow(customer);
            if (addEditWindow.ShowDialog() == true)
            {
                LoadCustomers(); // Refresh the list
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgCustomers.SelectedItem is Customer customer)
            {
                var result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa khách hàng '{customer.CompanyName}'?\n\n" +
                    "Lưu ý: Không thể xóa khách hàng đã có đơn hàng.",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        lblStatus.Text = "Đang xóa khách hàng...";

                        bool deleted = await _customerService.DeleteCustomerAsync(customer.CustomerId);

                        if (deleted)
                        {
                            _customers.Remove(customer);
                            _allCustomers.Remove(customer);
                            UpdateStatusBar();
                            lblStatus.Text = "Xóa khách hàng thành công";

                            MessageBox.Show("Xóa khách hàng thành công!",
                                          "Thành công",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa khách hàng: {ex.Message}",
                                      "Lỗi",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        lblStatus.Text = "Lỗi xóa khách hàng";
                    }
                }
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            LoadCustomers();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
