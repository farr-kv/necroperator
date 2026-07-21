using Microsoft.Extensions.DependencyInjection;
using Necroperator.Services;
using Necroperator.Services.Implementations;
using Necroperator.UI.Windows.Main;
using Serilog;
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
            var fileLogger = new LoggerConfiguration()
                .WriteTo.File(@"Logs\.log", rollingInterval: RollingInterval.Day, retainedFileTimeLimit: TimeSpan.FromDays(14), rollOnFileSizeLimit: true)
                .CreateLogger();

            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(fileLogger, dispose: true));

            // Services
            services.AddSingleton<IEventBus, EventBus>();
            services.AddSingleton<IBackupManager, BackupManager>();
            services.AddTransient<IFileMonitor, FileMonitor>();
            services.AddTransient<IUbisoftService, UbisoftService>();

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
            logger?.Publish(UIEvents.Error($"Unhandled exception: {e.Exception.Message}"));
        }
    }

}
