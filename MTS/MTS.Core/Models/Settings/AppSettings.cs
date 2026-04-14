namespace MTS.Core.Models.Settings;

public class AppSettings
{
    public TesterProfile TesterProfile { get; set; } = new();
    public UrlConfig Urls { get; set; } = new();

    // Editable lookup tables
    public List<Show> Shows { get; set; } = new();
    public List<CallType> CallTypes { get; set; } = new();
    public DonorList Donors { get; set; } = new();
    public List<SupervisorReason> SupervisorReasons { get; set; } = new();
    public List<FailReason> FailReasons { get; set; } = new();
    public List<CoachingCategory> CoachingCategories { get; set; } = new();

    // Integrations
    public PaymentConfig Payment { get; set; } = new();
    public GeminiConfig Gemini { get; set; } = new();
    public SheetsConfig Sheets { get; set; } = new();
    public CalendarConfig Calendar { get; set; } = new();
    public DiscordConfig Discord { get; set; } = new();

    // Preferences
    public UiPreferences UiPreferences { get; set; } = new();

    // Lifecycle flags
    public bool SetupComplete { get; set; }
    public bool TutorialCompleted { get; set; }
}
