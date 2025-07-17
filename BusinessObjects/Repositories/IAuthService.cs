    using BusinessObjects.Models;
    using ViewModels;

    namespace Repositories
    {
        public interface IAuthService
        {
            Task<object?> LoginAsync(LoginViewModel model);

            Task<bool> IsValidEmployeeAsync(string username, string password);
            Task<Employee?> GetCurrentEmployeeAsync(string username);

        }
    }
