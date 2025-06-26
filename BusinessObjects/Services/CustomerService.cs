using BusinessObjects.Models;
using Repositories;

namespace Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _customerRepository.GetAllCustomersAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int customerId)
        {
            return await _customerRepository.GetCustomerByIdAsync(customerId);
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            await ValidateCustomerAsync(customer);
            return await _customerRepository.AddCustomerAsync(customer);
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            await ValidateCustomerAsync(customer, true);
            return await _customerRepository.UpdateCustomerAsync(customer);
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            return await _customerRepository.DeleteCustomerAsync(customerId);
        }

        public async Task<List<Customer>> SearchCustomersAsync(string searchTerm)
        {
            return await _customerRepository.SearchCustomersAsync(searchTerm);
        }

        public async Task<bool> ValidateCustomerAsync(Customer customer, bool isUpdate = false)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(customer.CompanyName))
                throw new ArgumentException("Tên công ty là bắt buộc.");

            if (customer.CompanyName.Length > 40)
                throw new ArgumentException("Tên công ty không được vượt quá 40 ký tự.");

            // Validate optional fields length
            if (!string.IsNullOrEmpty(customer.ContactName) && customer.ContactName.Length > 30)
                throw new ArgumentException("Tên liên hệ không được vượt quá 30 ký tự.");

            if (!string.IsNullOrEmpty(customer.ContactTitle) && customer.ContactTitle.Length > 30)
                throw new ArgumentException("Chức vụ liên hệ không được vượt quá 30 ký tự.");

            if (!string.IsNullOrEmpty(customer.Address) && customer.Address.Length > 60)
                throw new ArgumentException("Địa chỉ không được vượt quá 60 ký tự.");

            if (!string.IsNullOrEmpty(customer.Phone) && customer.Phone.Length > 24)
                throw new ArgumentException("Số điện thoại không được vượt quá 24 ký tự.");

            // Check duplicate company name
            var excludeId = isUpdate ? customer.CustomerId : (int?)null;
            if (await _customerRepository.CompanyNameExistsAsync(customer.CompanyName, excludeId))
                throw new ArgumentException("Tên công ty đã tồn tại.");

            return true;
        }
    }
}
