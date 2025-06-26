using BusinessObjects.Models;
using ViewModels;

namespace Repositories
{
    public interface IAuthService
    {
        Task<Employee?> LoginAsync(LoginViewModel loginModel);
        Task<bool> IsValidEmployeeAsync(string username, string password);
        Task<Employee?> GetCurrentEmployeeAsync(string username);
    }
}
