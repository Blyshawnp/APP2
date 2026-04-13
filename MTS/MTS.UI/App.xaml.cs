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
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(ConfigureServices)
            .Build();

        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
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
        services.AddTransient<CallsViewModel>();
        services.AddTransient<SupervisorTransferViewModel>();
        services.AddTransient<ReviewViewModel>();
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
