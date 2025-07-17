using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.ViewModels;

namespace BusinessObjects.Services
{
    public interface ICustomerOrderService
    {
        Task<List<CustomerOrderDTO>> GetOrdersByCustomerIdAsync(int customerId);
    }
}
