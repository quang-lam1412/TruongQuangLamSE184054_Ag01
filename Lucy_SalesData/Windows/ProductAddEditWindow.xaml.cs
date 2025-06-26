using BusinessObjects.Models;
using Microsoft.Extensions.DependencyInjection;
using Services;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lucy_SalesData.Windows
{
    public partial class ProductAddEditWindow : Window
    {
        private readonly IProductService _productService;
        private readonly List<Category> _categories;
        private readonly Product? _editingProduct;
        private readonly bool _isEditMode;

        public ProductAddEditWindow(List<Category> categories, Product? product = null)
        {
            InitializeComponent();

            // Dependency Injection
            var serviceProvider = ((App)Application.Current).ServiceProvider;
            _productService = serviceProvider.GetRequiredService<IProductService>();

            _categories = categories;
            _editingProduct = product;
            _isEditMode = product != null;

            InitializeForm();
        }

        private void InitializeForm()
        {
            // Setup title
            lblTitle.Text = _isEditMode ? "📦 SỬA THÔNG TIN SẢN PHẨM" : "📦 THÊM SẢN PHẨM MỚI";
            Title = _isEditMode ? "Sửa Sản Phẩm" : "Thêm Sản Phẩm";

            // Setup category combobox
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add(new ComboBoxItem { Content = "-- Chọn danh mục --", Tag = (int?)null });

            foreach (var category in _categories)
            {
                cmbCategory.Items.Add(new ComboBoxItem { Content = category.CategoryName, Tag = category.CategoryId });
            }
            cmbCategory.SelectedIndex = 0;

            // Fill form if editing
            if (_isEditMode && _editingProduct != null)
            {
                FillForm(_editingProduct);
            }
            else
            {
                // Set default values for new product
                txtUnitPrice.Text = "0";
                txtUnitsInStock.Text = "0";
                txtUnitsOnOrder.Text = "0";
                txtReorderLevel.Text = "0";
            }
        }

        private void FillForm(Product product)
        {
            txtProductName.Text = product.ProductName;
            txtQuantityPerUnit.Text = product.QuantityPerUnit ?? "";
            txtUnitPrice.Text = product.UnitPrice?.ToString() ?? "0";
            txtUnitsInStock.Text = product.UnitsInStock?.ToString() ?? "0";
            txtUnitsOnOrder.Text = product.UnitsOnOrder?.ToString() ?? "0";
            txtReorderLevel.Text = product.ReorderLevel?.ToString() ?? "0";
            chkDiscontinued.IsChecked = product.Discontinued;

            // Select category
            if (product.CategoryId.HasValue)
            {
                for (int i = 0; i < cmbCategory.Items.Count; i++)
                {
                    if (cmbCategory.Items[i] is ComboBoxItem item &&
                        item.Tag is int categoryId &&
                        categoryId == product.CategoryId.Value)
                    {
                        cmbCategory.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numbers
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private bool ValidateForm()
        {
            // Product name is required
            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên sản phẩm.",
                              "Thông tin không hợp lệ",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                txtProductName.Focus();
                return false;
            }

            // Unit price validation
            if (!decimal.TryParse(txtUnitPrice.Text, out decimal unitPrice) || unitPrice < 0)
            {
                MessageBox.Show("Giá bán phải là số không âm.",
                              "Thông tin không hợp lệ",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                txtUnitPrice.Focus();
                return false;
            }

            // Units in stock validation
            if (!int.TryParse(txtUnitsInStock.Text, out int unitsInStock) || unitsInStock < 0)
            {
                MessageBox.Show("Số lượng tồn kho phải là số nguyên không âm.",
                              "Thông tin không hợp lệ",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                txtUnitsInStock.Focus();
                return false;
            }

            return true;
        }

        private Product CreateProductFromForm()
        {
            var product = _isEditMode ? _editingProduct! : new Product();

            product.ProductName = txtProductName.Text.Trim();
            product.QuantityPerUnit = string.IsNullOrWhiteSpace(txtQuantityPerUnit.Text) ? null : txtQuantityPerUnit.Text.Trim();
            product.UnitPrice = decimal.TryParse(txtUnitPrice.Text, out decimal price) ? price : 0;
            product.UnitsInStock = int.TryParse(txtUnitsInStock.Text, out int stock) ? stock : 0;
            product.UnitsOnOrder = int.TryParse(txtUnitsOnOrder.Text, out int onOrder) ? onOrder : 0;
            product.ReorderLevel = int.TryParse(txtReorderLevel.Text, out int reorder) ? reorder : 0;
            product.Discontinued = chkDiscontinued.IsChecked ?? false;

            // Set category
            if (cmbCategory.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is int categoryId)
            {
                product.CategoryId = categoryId;
            }
            else
            {
                product.CategoryId = null;
            }

            return product;
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                btnSave.IsEnabled = false;
                btnSave.Content = "⏳ Đang lưu...";

                var product = CreateProductFromForm();

                if (_isEditMode)
                {
                    await _productService.UpdateProductAsync(product);
                    MessageBox.Show("Cập nhật sản phẩm thành công!",
                                  "Thành công",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
                else
                {
                    await _productService.CreateProductAsync(product);
                    MessageBox.Show("Thêm sản phẩm mới thành công!",
                                  "Thành công",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu sản phẩm: {ex.Message}",
                              "Lỗi",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                btnSave.IsEnabled = true;
                btnSave.Content = "💾 Lưu";
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
