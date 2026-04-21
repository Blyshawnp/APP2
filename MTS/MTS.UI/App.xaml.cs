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
            if (args.ExceptionObject is Exception ex)
            {
                // Marshal to UI thread if available to avoid cross-thread access
                if (Application.Current?.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(ex.Message, "Fatal Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
                else
                {
                    // Fallback: just log if dispatcher not available
                    Debug.WriteLine($"[Domain] Cannot show UI - dispatcher unavailable");
                }
            }
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            Debug.WriteLine($"[Task] Unobserved: {args.Exception}");

            // Surface error to user via notification service (when host is ready)
            try
            {
                if (_host?.Services != null)
                {
                    var notificationService = _host.Services.GetService<INotificationService>();
                    notificationService?.ShowError("Background task failed. Check logs for details.");
                }
            }
            catch
            {
                // Avoid throwing in exception handler
            }

            args.SetObserved();
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
                await _host.StopAsync();
                _host.Dispose();
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