using BusinessObjects.Models;
using Microsoft.Extensions.DependencyInjection;
using Services;
using System.Windows;

namespace Lucy_SalesData.Windows
{
    public partial class CustomerAddEditWindow : Window
    {
        private readonly ICustomerService _customerService;
        private Customer? _customer;
        private bool _isEditMode;

        public CustomerAddEditWindow(Customer? customer = null)
        {
            InitializeComponent();

            // Dependency Injection
            var serviceProvider = ((App)Application.Current).ServiceProvider;
            _customerService = serviceProvider.GetRequiredService<ICustomerService>();

            _customer = customer;
            _isEditMode = customer != null;

            InitializeForm();
        }

        private void InitializeForm()
        {
            if (_isEditMode && _customer != null)
            {
                // Edit mode
                lblTitle.Text = "✏️ SỬA THÔNG TIN KHÁCH HÀNG";
                this.Title = "Sửa Khách Hàng";

                // Load existing data
                txtCompanyName.Text = _customer.CompanyName;
                txtContactName.Text = _customer.ContactName ?? "";
                txtContactTitle.Text = _customer.ContactTitle ?? "";
                txtAddress.Text = _customer.Address ?? "";
                txtPhone.Text = _customer.Phone ?? "";
            }
            else
            {
                // Add mode
                lblTitle.Text = "➕ THÊM KHÁCH HÀNG MỚI";
                this.Title = "Thêm Khách Hàng";
            }

            // Focus on first input
            txtCompanyName.Focus();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(txtCompanyName.Text))
                {
                    MessageBox.Show("Vui lòng nhập tên công ty.",
                                  "Thông tin thiếu",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    txtCompanyName.Focus();
                    return;
                }

                // Disable save button to prevent double-click
                btnSave.IsEnabled = false;
                btnSave.Content = "Đang lưu...";

                Customer customerToSave;

                if (_isEditMode && _customer != null)
                {
                    // Update existing customer
                    customerToSave = _customer;
                }
                else
                {
                    // Create new customer
                    customerToSave = new Customer();
                }

                // Set values
                customerToSave.CompanyName = txtCompanyName.Text.Trim();
                customerToSave.ContactName = string.IsNullOrWhiteSpace(txtContactName.Text) ? null : txtContactName.Text.Trim();
                customerToSave.ContactTitle = string.IsNullOrWhiteSpace(txtContactTitle.Text) ? null : txtContactTitle.Text.Trim();
                customerToSave.Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim();
                customerToSave.Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim();

                // Save to database
                if (_isEditMode)
                {
                    await _customerService.UpdateCustomerAsync(customerToSave);
                    MessageBox.Show("Cập nhật thông tin khách hàng thành công!",
                                  "Thành công",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
                else
                {
                    await _customerService.CreateCustomerAsync(customerToSave);
                    MessageBox.Show("Thêm khách hàng mới thành công!",
                                  "Thành công",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }

                // Close dialog with success result
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu thông tin khách hàng: {ex.Message}",
                              "Lỗi",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable save button
                btnSave.IsEnabled = true;
                btnSave.Content = "💾 Lưu";
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Check if there are unsaved changes
            if (HasUnsavedChanges())
            {
                var result = MessageBox.Show("Bạn có thay đổi chưa được lưu. Bạn có chắc chắn muốn thoát?",
                                           "Xác nhận thoát",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                    return;
            }

            DialogResult = false;
            Close();
        }

        private bool HasUnsavedChanges()
        {
            if (_isEditMode && _customer != null)
            {
                return txtCompanyName.Text.Trim() != _customer.CompanyName ||
                       txtContactName.Text.Trim() != (_customer.ContactName ?? "") ||
                       txtContactTitle.Text.Trim() != (_customer.ContactTitle ?? "") ||
                       txtAddress.Text.Trim() != (_customer.Address ?? "") ||
                       txtPhone.Text.Trim() != (_customer.Phone ?? "");
            }
            else
            {
                return !string.IsNullOrWhiteSpace(txtCompanyName.Text) ||
                       !string.IsNullOrWhiteSpace(txtContactName.Text) ||
                       !string.IsNullOrWhiteSpace(txtContactTitle.Text) ||
                       !string.IsNullOrWhiteSpace(txtAddress.Text) ||
                       !string.IsNullOrWhiteSpace(txtPhone.Text);
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult != true && HasUnsavedChanges())
            {
                var result = MessageBox.Show("Bạn có thay đổi chưa được lưu. Bạn có chắc chắn muốn thoát?",
                                           "Xác nhận thoát",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            base.OnClosing(e);
        }
    }
}
