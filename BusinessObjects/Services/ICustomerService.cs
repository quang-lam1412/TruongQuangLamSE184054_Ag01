using BusinessObjects.Models;

namespace Services
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(int customerId);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int customerId);
        Task<List<Customer>> SearchCustomersAsync(string searchTerm);
        Task<bool> ValidateCustomerAsync(Customer customer, bool isUpdate = false);
    }
}
