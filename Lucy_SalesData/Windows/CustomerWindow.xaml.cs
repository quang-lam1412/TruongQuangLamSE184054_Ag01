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
using Lucy_SalesData;
using Lucy_SalesData.Windows;
using TrươngQuangLâmWPF.Windows;
using AppContext = Lucy_SalesData.App;

namespace TruongQuangLamWPF.Windows
{
    /// <summary>
    /// Interaction logic for CustomerWindow.xaml
    /// </summary>
    public partial class CustomerWindow : Window
    {
        public CustomerWindow()
        {
            InitializeComponent();
        }
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            Application.Current.MainWindow = loginWindow; // chuyển quyền lại

            loginWindow.Show();
            this.Close(); // đóng cửa sổ hiện tại
        }
        //private void btnViewOrders_Click(object sender, RoutedEventArgs e)
        //{
        //    if (App.CurrentCustomer != null)
        //    {
        //        var customerId = App.CurrentCustomer.CustomerId;
        //        var orderWindow = new CustomerOrderWindow(customerId);
        //        orderWindow.ShowDialog(); // hoặc .Show()
        //    }
        //}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentCustomer != null)
            {
                var customerId = App.CurrentCustomer.CustomerId;
                var orderWindow = new CustomerOrderWindow(customerId);
                orderWindow.ShowDialog(); // hoặc .Show()
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var updateProfileWindow = new UpdateProfileWindow();
            updateProfileWindow.ShowDialog();
        }
    }
}
