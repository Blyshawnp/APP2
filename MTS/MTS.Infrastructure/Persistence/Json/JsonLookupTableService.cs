using System.Reflection;
using System.Text.Json;
using MTS.Core.Models.Settings;

namespace MTS.Infrastructure.Persistence.Json;

/// <summary>
/// Loads default lookup data from JSON files embedded in this assembly.
/// Used to seed AppSettings on first run.
/// Not editable at runtime — only seeds; user edits go through JsonSettingsService.
/// </summary>
public class JsonLookupTableService
{
    private static readonly Assembly _assembly = typeof(JsonLookupTableService).Assembly;

    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public List<CoachingCategory> LoadDefaultCoachingCategories()
        => LoadEmbedded<List<CoachingCategory>>("DefaultCoachingCategories.json")
           ?? new List<CoachingCategory>();

    public List<FailReason> LoadDefaultFailReasons()
        => LoadEmbedded<List<FailReason>>("DefaultFailReasons.json")
           ?? new List<FailReason>();

    public List<CallType> LoadDefaultCallTypes()
        => LoadEmbedded<List<CallType>>("DefaultCallTypes.json")
           ?? new List<CallType>();

    public List<SupervisorReason> LoadDefaultSupervisorReasons()
        => LoadEmbedded<List<SupervisorReason>>("DefaultSupervisorReasons.json")
           ?? new List<SupervisorReason>();

    public List<Show> LoadDefaultShows()
        => LoadEmbedded<List<Show>>("DefaultShows.json")
           ?? new List<Show>();

    public DonorList LoadDefaultDonors()
        => LoadEmbedded<DonorList>("DefaultDonors.json")
           ?? new DonorList();

    public AppSettings BuildDefaultSettings()
    {
        return new AppSettings
        {
            CoachingCategories = LoadDefaultCoachingCategories(),
            FailReasons        = LoadDefaultFailReasons(),
            CallTypes          = LoadDefaultCallTypes(),
            SupervisorReasons  = LoadDefaultSupervisorReasons(),
            Shows              = LoadDefaultShows(),
            Donors             = LoadDefaultDonors()
        };
    }

    private T? LoadEmbedded<T>(string fileName)
    {
        // Resource names follow the assembly's default namespace + folder path
        var resourceName = _assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
            throw new InvalidOperationException(
                $"Embedded resource '{fileName}' not found in {_assembly.FullName}.");

        using var stream = _assembly.GetManifestResourceStream(resourceName)!;
        return JsonSerializer.Deserialize<T>(stream, _options);
    }
}
