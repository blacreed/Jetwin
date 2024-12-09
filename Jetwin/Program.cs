using System;
using System.Windows.Forms;
using Jetwin.Models;
using Jetwin.Models.Interfaces;
using Jetwin.Modules;
using Jetwin.Presenter;
using Jetwin.Views;
using Jetwin.Views.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jetwin
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; }
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ServiceProvider = CreateHost().Services;

            var loginForm = ServiceProvider.GetRequiredService<frmLogin>();
            Application.Run(loginForm);
        }

        //https://stackoverflow.com/questions/70475830/how-to-use-dependency-injection-in-winforms
        private static IHost CreateHost()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    RegisterServices(services);
                }).Build();
        }
        private static void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<frmLogin>();
            services.AddTransient<MainForm>();

            RegisterModules(services);
        }
        private static void RegisterModules(IServiceCollection services)
        {
            // Dashboard Module
            services.AddTransient<DashboardView>()
                    .AddTransient<IDashboardView, DashboardView>()
                    .AddScoped<IDashboardRepository, DashboardRepository>()
                    .AddTransient<DashboardPresenter>();

            // Sales Module
            services.AddTransient<SalesView>()
                    .AddTransient<INewTransactionView, SalesView>()
                    .AddTransient<ISalesView, SalesView>()
                    .AddScoped<ISalesRepository, SalesRepository>()
                    .AddTransient<SalesPresenter>()
                    .AddScoped<ITransactionRepository, TransactionRepository>()
                    .AddTransient<NewTransactionPresenter>();

            // Maintenance Module
            services.AddTransient<MaintenanceView>()
                    .AddTransient<IMaintenanceView, MaintenanceView>()
                    .AddScoped<IUserRepository, UserRepository>()
                    .AddTransient<UserPresenter>()
                    .AddScoped<IProductRepository, ProductRepository>()
                    .AddTransient<ProductPresenter>()
                    .AddScoped<IAttributeRepository, AttributeRepository>()
                    .AddTransient<AttributePresenter>()
                    .AddScoped<ISupplierRepository, SupplierRepository>()
                    .AddTransient<SupplierPresenter>()
                    .AddScoped<IInventoryRepository, InventoryRepository>()
                    .AddTransient<InventoryPresenter>();

            // Inventory Module
            services.AddTransient<InventoryView>();
            services.AddTransient<InventoryModulePresenter>();

            // Additional modules...
            services.AddTransient<PurchaseOrderView>()
                        .AddTransient<ReportView>()
                        .AddTransient<MaintenanceView>()
                        .AddTransient<ProfileView>();
        }

    }
}
