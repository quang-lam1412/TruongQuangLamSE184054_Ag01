using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly LucySalesDataContext _context;

        public CustomerRepository(LucySalesDataContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .OrderBy(c => c.CompanyName)
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int customerId)
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }

        public async Task<Customer> AddCustomerAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            _context.Entry(customer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<bool> DeleteCustomerAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
                return false;

            // Kiểm tra xem khách hàng có đơn hàng không
            var hasOrders = await _context.Orders.AnyAsync(o => o.CustomerId == customerId);
            if (hasOrders)
                throw new InvalidOperationException("Không thể xóa khách hàng đã có đơn hàng.");

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Customer>> SearchCustomersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllCustomersAsync();

            searchTerm = searchTerm.ToLower();
            return await _context.Customers
                .Where(c => c.CompanyName.ToLower().Contains(searchTerm) ||
                           (c.ContactName != null && c.ContactName.ToLower().Contains(searchTerm)) ||
                           (c.Phone != null && c.Phone.Contains(searchTerm)))
                .OrderBy(c => c.CompanyName)
                .ToListAsync();
        }

        public async Task<bool> CustomerExistsAsync(int customerId)
        {
            return await _context.Customers.AnyAsync(c => c.CustomerId == customerId);
        }

        public async Task<bool> CompanyNameExistsAsync(string companyName, int? excludeCustomerId = null)
        {
            var query = _context.Customers.Where(c => c.CompanyName.ToLower() == companyName.ToLower());

            if (excludeCustomerId.HasValue)
                query = query.Where(c => c.CustomerId != excludeCustomerId.Value);

            return await query.AnyAsync();
        }
    }
}
