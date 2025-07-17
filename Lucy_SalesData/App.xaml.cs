using BusinessObjects.Models;
using BusinessObjects.Services;
using BusinessObjects.ViewModels;
using DataAccessLayer;
using Lucy_SalesData.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using Services;
using System.IO;
using System.Windows;
using TruongQuangLamWPF.Windows;
using AppContext = Lucy_SalesData.App;

namespace Lucy_SalesData
{
    public partial class App : Application
    {
        private static ServiceProvider? _serviceProvider;
        public static IServiceProvider ServiceProvider => _serviceProvider!;
        public static Employee? CurrentEmployee { get; set; }
        public static Customer? CurrentCustomer { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Đặt shutdown mode
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Setup Dependency Injection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<LucySalesDataContext>();
                var customerCount = db.Customers.Count();
                Console.WriteLine($"✅ Kết nối DB thành công! Số khách hàng: {customerCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi kết nối DB: {ex.Message}");
            }

            // Show Login Window
            ShowLoginWindow();
        }

        private void ShowLoginWindow()
        {
            var loginWindow = new LoginWindow();
            var result = loginWindow.ShowDialog();

            if (result == true && loginWindow.LoggedInEmployee != null)
            {
                CurrentEmployee = loginWindow.LoggedInEmployee;
                var mainWindow = new MainWindow();
                MainWindow = mainWindow;
                mainWindow.Show();
                ShutdownMode = ShutdownMode.OnMainWindowClose;
            }
            else if (result == true && loginWindow.LoggedInCustomer != null)
            {
                CurrentCustomer = loginWindow.LoggedInCustomer; // <== Gán ở đây
                var customerWindow = new CustomerWindow();
                MainWindow = customerWindow;
                customerWindow.Show();
                ShutdownMode = ShutdownMode.OnMainWindowClose;
            }

            else
            {
                Shutdown(); // login thất bại hoặc bấm hủy
            }
        }




        private void ConfigureServices(IServiceCollection services)
        {
            // Configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);

            // DbContext - QUAN TRỌNG: Đổi thành Scoped
            services.AddDbContext<LucySalesDataContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")),
                    ServiceLifetime.Scoped);


            // Repositories - Scoped
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();

            // Services - Scoped
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ICustomerOrderService, CustomerOrderService>();

            // ViewModels - Scoped
            services.AddScoped<CustomerOrderViewModel>();
            services.AddScoped<LoginWindow>();


        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
       


    }
}
