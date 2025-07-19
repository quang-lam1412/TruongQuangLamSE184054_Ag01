using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BusinessObjects.Models;
using Lucy_SalesData;
using Services;
using TruongQuangLamWPF.Windows;
using Microsoft.Extensions.DependencyInjection;
using AppContext = Lucy_SalesData.App;

namespace TrươngQuangLâmWPF.Windows
{
    /// <summary>
    /// Interaction logic for UpdateProfileWindow.xaml
    /// </summary>
    public partial class UpdateProfileWindow : Window
    {
        private readonly ICustomerService _customerService;
        private readonly Customer _currentCustomer;

        public UpdateProfileWindow()
        {
            InitializeComponent();

            _customerService = AppContext.ServiceProvider.GetRequiredService<ICustomerService>();
            _currentCustomer = AppContext.CurrentCustomer!;



            LoadCustomerData();
        }

        private void LoadCustomerData()
        {
            txtName.Text = _currentCustomer.ContactName ?? "";
            txtPhone.Text = _currentCustomer.Phone ?? "";
            txtAddress.Text = _currentCustomer.Address ?? "";
            txtCompanyName.Text = _currentCustomer.CompanyName ?? "";
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            new CustomerWindow().Show();
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            _currentCustomer.ContactName = txtName.Text.Trim();
            _currentCustomer.Phone = txtPhone.Text.Trim();
            _currentCustomer.Address = txtAddress.Text.Trim();
            _currentCustomer.CompanyName = txtCompanyName.Text.Trim();

            var updated = await _customerService.UpdateCustomerAsync(_currentCustomer);
            if (updated != null)
            {
                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
                new CustomerWindow().Show();
            }
            else
            {
                MessageBox.Show("Cập nhật thất bại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            new CustomerWindow().Show();
        }
    }
}
