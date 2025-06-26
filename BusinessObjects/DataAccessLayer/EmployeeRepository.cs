using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly LucySalesDataContext _context;

        public EmployeeRepository(LucySalesDataContext context)
        {
            _context = context;
        }

        public async Task<Employee?> GetEmployeeByUsernameAsync(string username)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.UserName == username);
        }

        public async Task<Employee?> ValidateEmployeeAsync(string username, string password)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.UserName == username && e.Password == password);
        }

        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            return await _context.Employees.ToListAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _context.Employees.FindAsync(id);
        }
    }
}
