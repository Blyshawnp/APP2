using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MTS.Infrastructure.DependencyInjection;
using MTS.UI.Services;
using MTS.UI.ViewModels;
using MTS.UI.ViewModels.Calls;
using MTS.UI.ViewModels.SupervisorTransfer;
using MTS.UI.ViewModels.Settings;

namespace MTS.UI;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        // Catch exceptions on all threads so they show a message instead of silently closing
        DispatcherUnhandledException += (_, ex) =>
        {
            MessageBox.Show(ex.Exception.ToString(), "Unhandled UI Exception",
                MessageBoxButton.OK, MessageBoxImage.Error);
            ex.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (_, ex) =>
            MessageBox.Show(ex.ExceptionObject?.ToString(), "Unhandled Exception",
                MessageBoxButton.OK, MessageBoxImage.Error);
        TaskScheduler.UnobservedTaskException += (_, ex) =>
        {
            MessageBox.Show(ex.Exception.ToString(), "Unobserved Task Exception",
                MessageBoxButton.OK, MessageBoxImage.Error);
            ex.SetObserved();
        };

        base.OnStartup(e);

        try
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();

            await _host.StartAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Startup Exception",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // ---- Infrastructure + Core (from MTS.Infrastructure) ----
        services.AddInfrastructure();

        // ---- UI Services ----
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<ISoundService, SoundService>();

        // ---- Shell ----
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();

        // ---- Screen ViewModels (Transient = fresh instance per navigation) ----
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<BasicsViewModel>();
        services.AddTransient<CallsViewModel>();
        services.AddTransient<SupervisorTransferViewModel>();
        services.AddTransient<ReviewViewModel>();
        services.AddTransient<NewbieShiftViewModel>();
        services.AddTransient<HistoryViewModel>();
        services.AddTransient<HelpViewModel>();
        services.AddTransient<DiscordPostViewModel>();
        services.AddTransient<SettingsViewModel>();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }
}

