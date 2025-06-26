using BusinessObjects.Models;
using DataAccessLayer;
using Lucy_SalesData.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using Services;
using System.IO;
using System.Windows;

namespace Lucy_SalesData
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;
        public IServiceProvider ServiceProvider => _serviceProvider!;
        public static Employee? CurrentEmployee { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Đặt shutdown mode
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Setup Dependency Injection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            // Show Login Window
            ShowLoginWindow();
        }

        private void ShowLoginWindow()
        {
            var loginWindow = new LoginWindow();
            var result = loginWindow.ShowDialog();

            if (result == true && loginWindow.LoggedInEmployee != null)
            {
                // Login successful, set current employee and show main window
                CurrentEmployee = loginWindow.LoggedInEmployee;
                var mainWindow = new MainWindow();
                MainWindow = mainWindow;
                mainWindow.Show();

                // Đặt lại shutdown mode về bình thường
                ShutdownMode = ShutdownMode.OnMainWindowClose;
            }
            else
            {
                // Login cancelled or failed, exit application
                Shutdown();
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
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
