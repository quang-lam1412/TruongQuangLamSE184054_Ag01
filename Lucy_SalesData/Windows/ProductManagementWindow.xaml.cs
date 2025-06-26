using BusinessObjects.Models;
using Microsoft.Extensions.DependencyInjection;
using Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lucy_SalesData.Windows
{
    public partial class ProductManagementWindow : Window
    {
        private readonly IProductService _productService;
        private ObservableCollection<Product> _products;
        private ObservableCollection<Product> _filteredProducts;
        private List<Category> _categories;

        public ProductManagementWindow()
        {
            InitializeComponent();

            // Dependency Injection
            var serviceProvider = ((App)Application.Current).ServiceProvider;
            _productService = serviceProvider.GetRequiredService<IProductService>();

            _products = new ObservableCollection<Product>();
            _filteredProducts = new ObservableCollection<Product>();
            _categories = new List<Category>();

            dgProducts.ItemsSource = _filteredProducts;

            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                lblStatus.Text = "Đang tải dữ liệu...";

                // Load categories
                var categories = await _productService.GetAllCategoriesAsync();
                _categories = categories.ToList();

                // Setup category combobox
                cmbCategory.Items.Clear();
                cmbCategory.Items.Add(new ComboBoxItem { Content = "Tất cả danh mục", Tag = -1 });
                foreach (var category in _categories)
                {
                    cmbCategory.Items.Add(new ComboBoxItem { Content = category.CategoryName, Tag = category.CategoryId });
                }
                cmbCategory.SelectedIndex = 0;

                // Load products
                await LoadProducts();

                lblStatus.Text = "Sẵn sàng";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}",
                              "Lỗi",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                lblStatus.Text = "Lỗi tải dữ liệu";
            }
        }

        private async Task LoadProducts()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();

                _products.Clear();
                foreach (var product in products)
                {
                    _products.Add(product);
                }

                ApplyFilters();
                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách sản phẩm: {ex.Message}",
                              "Lỗi",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            var filtered = _products.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchTerm = txtSearch.Text.ToLower();
                filtered = filtered.Where(p =>
                    p.ProductName.ToLower().Contains(searchTerm) ||
                    (p.Category?.CategoryName.ToLower().Contains(searchTerm) ?? false) ||
                    (p.QuantityPerUnit?.ToLower().Contains(searchTerm) ?? false));
            }

            // Apply category filter
            if (cmbCategory.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag is int categoryId && categoryId != -1)
            {
                filtered = filtered.Where(p => p.CategoryId == categoryId);
            }

            _filteredProducts.Clear();
            foreach (var product in filtered)
            {
                _filteredProducts.Add(product);
            }

            UpdateStatusBar();
        }

        private async void UpdateStatusBar()
        {
            lblTotalRecords.Text = $"Tổng: {_filteredProducts.Count} sản phẩm";

            // Count low stock products
            try
            {
                var lowStockProducts = await _productService.GetLowStockProductsAsync();
                lblLowStock.Text = $"Sắp hết hàng: {lowStockProducts.Count()}";
            }
            catch
            {
                lblLowStock.Text = "Sắp hết hàng: --";
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplyFilters();
            }
        }

        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            cmbCategory.SelectedIndex = 0;
            ApplyFilters();
        }

        private void DgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var hasSelection = dgProducts.SelectedItem != null;
            btnEdit.IsEnabled = hasSelection;
            btnDelete.IsEnabled = hasSelection;
        }

        private void DgProducts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgProducts.SelectedItem != null)
            {
                BtnEdit_Click(sender, e);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new ProductAddEditWindow(_categories);
            if (addWindow.ShowDialog() == true)
            {
                LoadProducts();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is Product selectedProduct)
            {
                var editWindow = new ProductAddEditWindow(_categories, selectedProduct);
                if (editWindow.ShowDialog() == true)
                {
                    LoadProducts();
                }
            }
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is Product selectedProduct)
            {
                var result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa sản phẩm '{selectedProduct.ProductName}'?\n\n" +
                    "Lưu ý: Không thể xóa sản phẩm đã có trong đơn hàng.",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        lblStatus.Text = "Đang xóa sản phẩm...";

                        await _productService.DeleteProductAsync(selectedProduct.ProductId);

                        MessageBox.Show("Xóa sản phẩm thành công!",
                                      "Thành công",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Information);

                        await LoadProducts();
                        lblStatus.Text = "Sẵn sàng";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi xóa sản phẩm: {ex.Message}",
                                      "Lỗi",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        lblStatus.Text = "Lỗi xóa sản phẩm";
                    }
                }
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadProducts();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
