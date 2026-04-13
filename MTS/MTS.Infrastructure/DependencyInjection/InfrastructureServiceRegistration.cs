using Microsoft.Extensions.DependencyInjection;
using MTS.Core.Interfaces.Repositories;
using MTS.Core.Interfaces.Services;
using MTS.Core.Rules;
using MTS.Core.Services;
using MTS.Infrastructure.ExternalServices;
using MTS.Infrastructure.Persistence.Json;
using MTS.Infrastructure.Persistence.LiteDb;

namespace MTS.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    /// <summary>
    /// Registers all infrastructure and core application services.
    /// Call this from App.xaml.cs before building the host.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // ---- Lookup / seed data ----
        services.AddSingleton<JsonLookupTableService>();

        // ---- Persistence ----
        // LiteDbSessionRepository implements both ISessionRepository and IHistoryRepository
        services.AddSingleton<LiteDbSessionRepository>();
        services.AddSingleton<ISessionRepository>(sp => sp.GetRequiredService<LiteDbSessionRepository>());
        services.AddSingleton<IHistoryRepository>(sp  => sp.GetRequiredService<LiteDbSessionRepository>());

        // ---- Settings (JSON file) ----
        services.AddSingleton<ISettingsService, JsonSettingsService>();

        // ---- Domain rules (stateless — shared singleton) ----
        services.AddSingleton<EvaluationRulesService>();
        services.AddSingleton<ValidationService>();

        // ---- Core application services ----
        services.AddSingleton<ISessionStateService, SessionStateService>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<IHistoryService, HistoryService>();

        // ---- External services (AI / no-op defaults) ----
        // Replace NullSummaryService with GeminiSummaryService in Phase 5
        services.AddSingleton<ISummaryService, NullSummaryService>();

        return services;
    }
}
