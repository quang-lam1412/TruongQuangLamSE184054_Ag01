using BusinessObjects.Models;
using Repositories;

namespace Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllProductsAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _productRepository.GetProductByIdAsync(productId);
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            return await _productRepository.SearchProductsAsync(searchTerm);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(product.ProductName))
                throw new ArgumentException("Tên sản phẩm không được để trống.");

            if (product.UnitPrice < 0)
                throw new ArgumentException("Giá sản phẩm không được âm.");

            if (product.UnitsInStock < 0)
                throw new ArgumentException("Số lượng tồn kho không được âm.");

            return await _productRepository.CreateProductAsync(product);
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(product.ProductName))
                throw new ArgumentException("Tên sản phẩm không được để trống.");

            if (product.UnitPrice < 0)
                throw new ArgumentException("Giá sản phẩm không được âm.");

            if (product.UnitsInStock < 0)
                throw new ArgumentException("Số lượng tồn kho không được âm.");

            var existingProduct = await _productRepository.GetProductByIdAsync(product.ProductId);
            if (existingProduct == null)
                throw new ArgumentException("Sản phẩm không tồn tại.");

            return await _productRepository.UpdateProductAsync(product);
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            return await _productRepository.DeleteProductAsync(productId);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _productRepository.GetProductsByCategoryAsync(categoryId);
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
        {
            return await _productRepository.GetLowStockProductsAsync(threshold);
        }

        public async Task<IEnumerable<Product>> GetDiscontinuedProductsAsync()
        {
            return await _productRepository.GetDiscontinuedProductsAsync();
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllCategoriesAsync();
        }

        public async Task<int> GetTotalProductsCountAsync()
        {
            return await _productRepository.GetTotalProductsCountAsync();
        }
    }
}
