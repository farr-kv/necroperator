using Microsoft.Extensions.DependencyInjection;
using Necroperator.Services;
using Necroperator.Services.Implementations;
using Necroperator.UI.Windows.Main;
using System.Windows;

namespace Necroperator
{
    public partial class App : Application
    {
        private IServiceProvider? serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            serviceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            this.DispatcherUnhandledException += OnUnhandledException;

        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Services
            services.AddSingleton<IEventBus, EventBus>();
            services.AddTransient<IFileMonitor, FileMonitor>();
            services.AddTransient<IPeriodicBackupService, PeriodicBackupService>();

            // ViewModels
            services.AddTransient<MainWindowModel>();

            // Windows
            services.AddSingleton<MainWindow>();
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            this.DispatcherUnhandledException -= OnUnhandledException;
            if (serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private void OnUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var logger = serviceProvider?.GetService<IEventBus>();
            logger?.Publish(Events.Error($"Unhandled exception: {e.Exception.Message}"));
        }
    }

}
