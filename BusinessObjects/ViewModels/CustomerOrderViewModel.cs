using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Services;

namespace BusinessObjects.ViewModels
{
    public class CustomerOrderViewModel : INotifyPropertyChanged
    {
        private readonly ICustomerOrderService _orderService;

        public ObservableCollection<CustomerOrderDTO> Orders { get; set; } = new();

        public CustomerOrderViewModel(ICustomerOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task LoadOrdersAsync(int customerId)
        {
            var result = await _orderService.GetOrdersByCustomerIdAsync(customerId);
            Orders.Clear();
            foreach (var order in result)
            {
                Orders.Add(order);
            }
        }

        // Notify property changed boilerplate
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}