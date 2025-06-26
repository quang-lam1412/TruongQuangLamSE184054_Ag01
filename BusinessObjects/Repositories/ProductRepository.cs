using BusinessObjects.Models;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly LucySalesDataContext _context;

        public ProductRepository(LucySalesDataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.ProductName)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllProductsAsync();

            var lowerSearchTerm = searchTerm.ToLower();

            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.ProductName.ToLower().Contains(lowerSearchTerm) ||
                           (p.Category != null && p.Category.CategoryName.ToLower().Contains(lowerSearchTerm)) ||
                           (p.QuantityPerUnit != null && p.QuantityPerUnit.ToLower().Contains(lowerSearchTerm)))
                .OrderBy(p => p.ProductName)
                .ToListAsync();
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Load category information
            await _context.Entry(product)
                .Reference(p => p.Category)
                .LoadAsync();

            return product;
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            // Load category information
            await _context.Entry(product)
                .Reference(p => p.Category)
                .LoadAsync();

            return product;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            // Check if product has related order details
            var hasOrderDetails = await _context.OrderDetails
                .AnyAsync(od => od.ProductId == productId);

            if (hasOrderDetails)
            {
                throw new InvalidOperationException("Không thể xóa sản phẩm này vì đã có đơn hàng liên quan.");
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ProductExistsAsync(int productId)
        {
            return await _context.Products.AnyAsync(p => p.ProductId == productId);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .OrderBy(p => p.ProductName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.UnitsInStock <= threshold && !p.Discontinued)
                .OrderBy(p => p.UnitsInStock)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetDiscontinuedProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Discontinued)
                .OrderBy(p => p.ProductName)
                .ToListAsync();
        }

        public async Task<int> GetTotalProductsCountAsync()
        {
            return await _context.Products.CountAsync();
        }
    }
}
