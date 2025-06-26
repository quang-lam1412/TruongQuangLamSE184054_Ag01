using BusinessObjects.Models;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly LucySalesDataContext _context;

        public OrderRepository(LucySalesDataContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id)
                .ConfigureAwait(false);
        }

        public async Task<List<Order>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderDetails)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<List<Order>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderDetails)
                .Where(o => o.EmployeeId == employeeId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<List<Order>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderDetails)
                .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<Order> AddAsync(Order order)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ✅ CLEAR CHANGE TRACKER
                _context.ChangeTracker.Clear();

                // ✅ TẠO OrderId THỦ CÔNG
                var maxOrderId = await _context.Orders
                    .AsNoTracking()
                    .MaxAsync(o => (int?)o.OrderId) ?? 0;
                var newOrderId = maxOrderId + 1;

                // ✅ SET OrderId CHO ORDER
                order.OrderId = newOrderId;

                // ✅ SET OrderId CHO TẤT CẢ ORDER DETAILS
                if (order.OrderDetails != null && order.OrderDetails.Any())
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        detail.OrderId = newOrderId; // ✅ SET OrderId cho từng detail
                        detail.Order = null!;
                        detail.Product = null!;
                    }
                }

                // ✅ CLEAR NAVIGATION PROPERTIES
                order.Customer = null!;
                order.Employee = null!;

                System.Diagnostics.Debug.WriteLine($"Adding Order with manual ID: {newOrderId} - CustomerId: {order.CustomerId}, EmployeeId: {order.EmployeeId}");

                // ✅ ADD ORDER VÀ ORDER DETAILS
                _context.Orders.Add(order);

                // ✅ SAVE CHANGES
                await _context.SaveChangesAsync().ConfigureAwait(false);

                // ✅ COMMIT TRANSACTION
                await transaction.CommitAsync();

                System.Diagnostics.Debug.WriteLine($"Order created successfully with ID: {order.OrderId}");

                // ✅ LOAD LẠI ORDER VỚI FULL DATA
                return await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Employee)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .FirstAsync(o => o.OrderId == newOrderId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine($"OrderRepository.AddAsync Error: {ex}");
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                throw;
            }
        }


        public async Task<bool> UpdateAsync(Order order)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ✅ CLEAR CHANGE TRACKER
                _context.ChangeTracker.Clear();

                // ✅ TÌM ORDER HIỆN TẠI VỚI ORDER DETAILS
                var existingOrder = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

                if (existingOrder == null)
                    return false;

                // ✅ CẬP NHẬT THÔNG TIN ORDER
                existingOrder.CustomerId = order.CustomerId;
                existingOrder.EmployeeId = order.EmployeeId;
                existingOrder.OrderDate = order.OrderDate;

                // ✅ XÓA TẤT CẢ ORDER DETAILS CŨ
                if (existingOrder.OrderDetails != null && existingOrder.OrderDetails.Any())
                {
                    _context.OrderDetails.RemoveRange(existingOrder.OrderDetails);
                }

                // ✅ THÊM ORDER DETAILS MỚI
                if (order.OrderDetails != null && order.OrderDetails.Any())
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        var newDetail = new OrderDetail
                        {
                            OrderId = order.OrderId,
                            ProductId = detail.ProductId,
                            UnitPrice = detail.UnitPrice,
                            Quantity = detail.Quantity,
                            Discount = detail.Discount
                        };
                        _context.OrderDetails.Add(newDetail);
                    }
                }

                // ✅ SAVE CHANGES
                var result = await _context.SaveChangesAsync().ConfigureAwait(false);

                // ✅ COMMIT TRANSACTION
                await transaction.CommitAsync();

                System.Diagnostics.Debug.WriteLine($"Order {order.OrderId} updated successfully");

                return result > 0;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine($"UpdateAsync Error: {ex}");
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                throw;
            }
        }


        public async Task<bool> DeleteAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ✅ TÌM ORDER VỚI ORDER DETAILS
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                    return false;

                // ✅ XÓA ORDER DETAILS TRƯỚC
                if (order.OrderDetails != null && order.OrderDetails.Any())
                {
                    _context.OrderDetails.RemoveRange(order.OrderDetails);
                    System.Diagnostics.Debug.WriteLine($"Removing {order.OrderDetails.Count} order details for Order {id}");
                }

                // ✅ XÓA ORDER SAU
                _context.Orders.Remove(order);

                System.Diagnostics.Debug.WriteLine($"Removing Order {id}");

                // ✅ SAVE CHANGES
                var result = await _context.SaveChangesAsync().ConfigureAwait(false);

                // ✅ COMMIT TRANSACTION
                await transaction.CommitAsync();

                System.Diagnostics.Debug.WriteLine($"Order {id} deleted successfully. Affected rows: {result}");

                return result > 0;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                System.Diagnostics.Debug.WriteLine($"DeleteAsync Error: {ex}");
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                throw;
            }
        }


        public async Task<List<OrderDetail>> GetOrderDetailsAsync(int orderId)
        {
            return await _context.OrderDetails
                .Include(od => od.Product)
                .Where(od => od.OrderId == orderId)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<bool> AddOrderDetailAsync(OrderDetail orderDetail)
        {
            try
            {
                _context.OrderDetails.Add(orderDetail);
                var result = await _context.SaveChangesAsync().ConfigureAwait(false);
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateOrderDetailAsync(OrderDetail orderDetail)
        {
            try
            {
                _context.OrderDetails.Update(orderDetail);
                var result = await _context.SaveChangesAsync().ConfigureAwait(false);
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveOrderDetailAsync(int orderId, int productId)
        {
            try
            {
                var orderDetail = await _context.OrderDetails
                    .FirstOrDefaultAsync(od => od.OrderId == orderId && od.ProductId == productId)
                    .ConfigureAwait(false);

                if (orderDetail != null)
                {
                    _context.OrderDetails.Remove(orderDetail);
                    var result = await _context.SaveChangesAsync().ConfigureAwait(false);
                    return result > 0;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
