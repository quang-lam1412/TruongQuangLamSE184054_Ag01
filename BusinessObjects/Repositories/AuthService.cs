using BusinessObjects.Models;
using DataAccessLayer;
using ViewModels;

namespace Repositories
{
    public class AuthService : IAuthService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICustomerRepository _customerRepository;

        public AuthService(IEmployeeRepository employeeRepository, ICustomerRepository customerRepository)
        {
            _employeeRepository = employeeRepository;
            _customerRepository = customerRepository;
        }

        public async Task<object?> LoginAsync(LoginViewModel loginModel)
        {
            if (string.IsNullOrWhiteSpace(loginModel.Identifier) ||
                string.IsNullOrWhiteSpace(loginModel.Password))
            {
                return null;
            }

            if (loginModel.Role == UserRole.Admin)
            {
                return await _employeeRepository.ValidateEmployeeAsync(
                    loginModel.Identifier.Trim(),
                    loginModel.Password.Trim());
            }
            else // Customer
            {
                return await _customerRepository.ValidateCustomerAsync(
                    loginModel.Identifier.Trim(),
                    loginModel.Password.Trim());
            }
        }
        public async Task<bool> IsValidEmployeeAsync(string username, string password)
        {
            var employee = await _employeeRepository.ValidateEmployeeAsync(username, password);
            return employee != null;
        }

        public async Task<Employee?> GetCurrentEmployeeAsync(string username)
        {
            return await _employeeRepository.GetEmployeeByUsernameAsync(username);
        }

    }
}
