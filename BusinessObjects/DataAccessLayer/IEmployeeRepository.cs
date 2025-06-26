using BusinessObjects.Models;

namespace DataAccessLayer
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetEmployeeByUsernameAsync(string username);
        Task<Employee?> ValidateEmployeeAsync(string username, string password);
        Task<List<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(int id);
    }
}
