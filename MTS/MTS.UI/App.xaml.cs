using System.Diagnostics;
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
    private INotificationService? _notificationService;

    protected override async void OnStartup(StartupEventArgs e)
    {
        // Catch unhandled exceptions from all sources so the app never
        // silently vanishes — show a message then let the user decide.
        DispatcherUnhandledException += (_, args) =>
        {
            Debug.WriteLine($"[UI] Unhandled: {args.Exception}");
            MessageBox.Show(args.Exception.Message, "Unexpected Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            Debug.WriteLine($"[Domain] Unhandled: {args.ExceptionObject}");

            // Marshal UI interaction to the UI thread if available
            if (Application.Current?.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (args.ExceptionObject is Exception ex)
                        MessageBox.Show(ex.Message, "Fatal Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
            else
            {
                // Fallback if dispatcher is not available
                if (args.ExceptionObject is Exception ex)
                    MessageBox.Show(ex.Message, "Fatal Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            // Log the full exception for diagnostics
            Debug.WriteLine($"[Task] Unobserved: {args.Exception}");

            // Surface the error to the user via notification service if available
            if (_notificationService != null && Application.Current?.Dispatcher != null)
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    _notificationService.ShowError("Background task failed. Please check application logs.", 5000);
                });
            }

            // Mark as observed to prevent app termination
            args.SetObserved();
        };

        base.OnStartup(e);

        try
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();

            await _host.StartAsync();

            // Resolve notification service for use in exception handlers
            _notificationService = _host.Services.GetRequiredService<INotificationService>();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Startup] Fatal: {ex}");
            MessageBox.Show($"Failed to start MTS:\n\n{ex.Message}", "Startup Error",
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
        try
        {
            if (_host != null)
            {
                try
                {
                    await _host.StopAsync();
                }
                finally
                {
                    _host.Dispose();
                    _host = null;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Shutdown] {ex}");
        }
        finally
        {
            base.OnExit(e);
        }
    }
}