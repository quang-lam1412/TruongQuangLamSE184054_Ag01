﻿using BusinessObjects.Models;
using Lucy_SalesData.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AppContext = Lucy_SalesData.App;

namespace Lucy_SalesData.Windows
{
    public partial class EditOrderWindow : Window
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly int _orderId;

        private ObservableCollection<OrderDetailViewModel> _orderDetails = new();
        private List<Product> _products = new();
        private Order? _originalOrder;

        public EditOrderWindow(int orderId)
        {
            try
            {
                InitializeComponent();
                _orderId = orderId;

                // Dependency Injection
                var serviceProvider = AppContext.ServiceProvider;
                using var scope = serviceProvider.CreateScope();
                _orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                _customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();
                _productService = scope.ServiceProvider.GetRequiredService<IProductService>();

                InitializeWindow();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo EditOrderWindow: {ex.Message}", "Lỗi",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeWindow()
        {
            try
            {
                lblOrderId.Text = _orderId.ToString();

                if (lblEmployee != null)
                    lblEmployee.Text = App.CurrentEmployee?.Name ?? "N/A";

                if (dgOrderDetails != null)
                    dgOrderDetails.ItemsSource = _orderDetails;

                // Khởi tạo giá trị mặc định
                if (txtQuantity != null)
                    txtQuantity.Text = "1";

                if (txtDiscount != null)
                    txtDiscount.Text = "0";

                if (lblLineTotal != null)
                    lblLineTotal.Text = "0 VNĐ";

                if (lblGrandTotal != null)
                    lblGrandTotal.Text = "0 VNĐ";

                // Subscribe to collection changed event
                _orderDetails.CollectionChanged += (s, e) => UpdateGrandTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo window: {ex.Message}", "Lỗi",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadData()
        {
            await LoadCustomers();
            await LoadProducts();
            await LoadOrderData();
        }

        private async Task LoadCustomers()
        {
            try
            {
                var serviceProvider = AppContext.ServiceProvider;
                using var scope = serviceProvider.CreateScope();
                var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

                var customers = await customerService.GetAllCustomersAsync();

                Dispatcher.Invoke(() =>
                {
                    cbCustomer.ItemsSource = customers;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách khách hàng: {ex.Message}",
                              "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadProducts()
        {
            try
            {
                var serviceProvider = AppContext.ServiceProvider;
                using var scope = serviceProvider.CreateScope();
                var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

                _products = (await productService.GetAllProductsAsync()).ToList();

                Dispatcher.Invoke(() =>
                {
                    cbProduct.ItemsSource = _products;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách sản phẩm: {ex.Message}",
                              "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadOrderData()
        {
            try
            {
                var serviceProvider = AppContext.ServiceProvider;
                using var scope = serviceProvider.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                _originalOrder = await orderService.GetOrderByIdAsync(_orderId);

                if (_originalOrder == null)
                {
                    MessageBox.Show("Không tìm thấy đơn hàng!", "Lỗi",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                Dispatcher.Invoke(() =>
                {
                    // Set order info
                    cbCustomer.SelectedValue = _originalOrder.CustomerId;
                    dpOrderDate.SelectedDate = _originalOrder.OrderDate;

                    // Load order details
                    _orderDetails.Clear();
                    if (_originalOrder.OrderDetails != null)
                    {
                        foreach (var detail in _originalOrder.OrderDetails)
                        {
                            _orderDetails.Add(new OrderDetailViewModel
                            {
                                ProductId = detail.ProductId,
                                ProductName = detail.Product?.ProductName ?? "N/A",
                                UnitPrice = detail.UnitPrice,
                                Quantity = detail.Quantity,
                                Discount = detail.Discount
                            });
                        }
                    }

                    UpdateGrandTotal();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu đơn hàng: {ex.Message}",
                              "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CbProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cbProduct.SelectedItem is Product selectedProduct && txtUnitPrice != null)
                {
                    var unitPrice = selectedProduct.UnitPrice ?? 0;
                    txtUnitPrice.Text = unitPrice.ToString("N0");
                    UpdateLineTotal();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi chọn sản phẩm: {ex.Message}", "Lỗi",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateLineTotal()
        {
            try
            {
                if (lblLineTotal == null) return;

                if (decimal.TryParse(txtUnitPrice?.Text?.Replace(",", "") ?? "0", out decimal unitPrice) &&
                    int.TryParse(txtQuantity?.Text ?? "0", out int quantity) &&
                    float.TryParse(txtDiscount?.Text ?? "0", out float discount))
                {
                    var lineTotal = unitPrice * quantity * (decimal)(1 - discount / 100);
                    lblLineTotal.Text = $"{lineTotal:N0} VNĐ";
                }
                else
                {
                    lblLineTotal.Text = "0 VNĐ";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateLineTotal Error: {ex.Message}");
            }
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbProduct.SelectedItem is not Product selectedProduct)
                {
                    MessageBox.Show("Vui lòng chọn sản phẩm!", "Thông báo",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Số lượng phải là số nguyên dương!", "Thông báo",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtUnitPrice.Text.Replace(",", ""), out decimal unitPrice) || unitPrice <= 0)
                {
                    MessageBox.Show("Đơn giá phải lớn hơn 0!", "Thông báo",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!float.TryParse(txtDiscount.Text, out float discount) || discount < 0 || discount > 100)
                {
                    MessageBox.Show("Giảm giá phải từ 0 đến 100!", "Thông báo",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if product already exists
                var existingItem = _orderDetails.FirstOrDefault(od => od.ProductId == selectedProduct.ProductId);
                if (existingItem != null)
                {
                    MessageBox.Show("Sản phẩm đã có trong đơn hàng!", "Thông báo",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Add new order detail
                var orderDetail = new OrderDetailViewModel
                {
                    ProductId = selectedProduct.ProductId,
                    ProductName = selectedProduct.ProductName,
                    UnitPrice = unitPrice,
                    Quantity = (short)quantity,
                    Discount = discount / 100f
                };

                _orderDetails.Add(orderDetail);

                // Clear input fields
                cbProduct.SelectedIndex = -1;
                txtQuantity.Text = "1";
                txtUnitPrice.Text = "";
                txtDiscount.Text = "0";
                lblLineTotal.Text = "0 VNĐ";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi thêm sản phẩm: {ex.Message}", "Lỗi",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int productId)
            {
                var itemToRemove = _orderDetails.FirstOrDefault(od => od.ProductId == productId);
                if (itemToRemove != null)
                {
                    _orderDetails.Remove(itemToRemove);
                }
            }
        }

        private void UpdateGrandTotal()
        {
            try
            {
                if (lblGrandTotal == null) return;

                var grandTotal = _orderDetails?.Sum(od => od.LineTotal) ?? 0;
                lblGrandTotal.Text = $"{grandTotal:N0} VNĐ";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateGrandTotal Error: {ex.Message}");
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnSave.IsEnabled = false;
                btnCancel.IsEnabled = false;

                // ✅ VALIDATION
                if (cbCustomer.SelectedValue is not int customerId)
                {
                    MessageBox.Show("Vui lòng chọn khách hàng!", "Thông báo",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!_orderDetails.Any())
                {
                    MessageBox.Show("Vui lòng thêm ít nhất một sản phẩm!", "Thông báo",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_originalOrder == null)
                {
                    MessageBox.Show("Không tìm thấy thông tin đơn hàng gốc!", "Lỗi",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // ✅ UPDATE ORDER
                var updatedOrder = new Order
                {
                    OrderId = _orderId,
                    CustomerId = customerId,
                    EmployeeId = _originalOrder.EmployeeId, // Giữ nguyên nhân viên
                    OrderDate = dpOrderDate.SelectedDate ?? DateTime.Now,
                    OrderDetails = _orderDetails.Select(od => new OrderDetail
                    {
                        OrderId = _orderId,
                        ProductId = od.ProductId,
                        UnitPrice = od.UnitPrice,
                        Quantity = (short)od.Quantity,
                        Discount = (float)od.Discount
                    }).ToList()
                };

                // ✅ SAVE ORDER
                var serviceProvider = AppContext.ServiceProvider;
                using var scope = serviceProvider.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                var success = await orderService.UpdateOrderAsync(updatedOrder);

                if (success)
                {
                    MessageBox.Show($"Cập nhật đơn hàng thành công!\nMã đơn hàng: {_orderId}",
                                  "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Cập nhật đơn hàng thất bại!", "Lỗi",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                System.Diagnostics.Debug.WriteLine($"UpdateOrder Error: {ex}");
                MessageBox.Show($"Lỗi cập nhật đơn hàng: {errorMessage}", "Lỗi",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnSave.IsEnabled = true;
                btnCancel.IsEnabled = true;
            }
        }

        private void TxtQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                UpdateLineTotal();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TxtQuantity_TextChanged Error: {ex.Message}");
            }
        }

        private void TxtDiscount_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                UpdateLineTotal();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TxtDiscount_TextChanged Error: {ex.Message}");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
