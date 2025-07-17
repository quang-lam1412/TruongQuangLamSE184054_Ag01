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
using BusinessObjects.ViewModels;
using Lucy_SalesData;
using Microsoft.Extensions.DependencyInjection;
using TruongQuangLamWPF.Windows;

using AppContext = Lucy_SalesData.App;

namespace TrươngQuangLâmWPF.Windows
{
    /// <summary>
    /// Interaction logic for CustomerOrderWindow.xaml
    /// </summary>
    public partial class CustomerOrderWindow : Window
    {
        private readonly CustomerOrderViewModel _viewModel;

        public CustomerOrderWindow(int customerId)
        {
            InitializeComponent();

            // Lấy ViewModel từ DI container
            var orderViewModel = AppContext.ServiceProvider.GetRequiredService<CustomerOrderViewModel>();
            _viewModel = orderViewModel;

            DataContext = _viewModel;

            // Load đơn hàng cho khách
            _ = _viewModel.LoadOrdersAsync(customerId);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var customerWindow = new CustomerWindow();
            customerWindow.Show();
            this.Close();
        }
    }
}
