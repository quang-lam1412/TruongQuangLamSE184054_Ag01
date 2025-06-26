using BusinessObjects.Models;

namespace Repositories
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerByIdAsync(int customerId);
        Task<Customer> AddCustomerAsync(Customer customer);
        Task<Customer> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int customerId);
        Task<List<Customer>> SearchCustomersAsync(string searchTerm);
        Task<bool> CustomerExistsAsync(int customerId);
        Task<bool> CompanyNameExistsAsync(string companyName, int? excludeCustomerId = null);
    }
}
