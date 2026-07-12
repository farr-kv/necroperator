using Microsoft.Extensions.DependencyInjection;
using Necroperator.Services;
using Necroperator.Services.Implementations;
using Necroperator.UI.ViewModels;
using Necroperator.UI.ViewModels.Implementations;
using System.Windows;

namespace Necroperator
{
    public partial class App : Application
    {
        private IServiceProvider serviceProvider;

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
            services.AddSingleton<IEventBus, EventBus>();
            services.AddTransient<IFileMonitor, FileMonitor>();
            services.AddTransient<IPeriodicBackupService, PeriodicBackupService>();

            services.AddTransient<IMainWindowModel, MainWindowModel>();

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
            var logger = serviceProvider.GetService<IEventBus>();
            logger?.Publish(Events.Error($"Unhandled exception: {e.Exception.Message}"));
        }
    }

}
