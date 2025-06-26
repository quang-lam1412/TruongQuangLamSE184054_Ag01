using BusinessObjects.Models;
using DataAccessLayer;
using ViewModels;

namespace Repositories
{
    public class AuthService : IAuthService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public AuthService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<Employee?> LoginAsync(LoginViewModel loginModel)
        {
            if (string.IsNullOrWhiteSpace(loginModel.UserName) ||
                string.IsNullOrWhiteSpace(loginModel.Password))
            {
                return null;
            }

            return await _employeeRepository.ValidateEmployeeAsync(
                loginModel.UserName.Trim(),
                loginModel.Password.Trim());
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
