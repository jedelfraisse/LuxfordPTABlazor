namespace LuxfordPTAWeb.Shared.DTOs;

public class TalentShowRehearsalPackage
{
    public string ShowName { get; set; } = string.Empty;
    public string SessionCode { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public DateTime ShowDate { get; set; }
    public List<TalentShowScheduleItem> Items { get; set; } = new();
    public TalentShowPresentationSettings Presentation { get; set; } = new();
}

public class TalentShowScheduleItem
{
    public int Order { get; set; }
    public string PerformerName { get; set; } = string.Empty;
    public string ActTitle { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string IntroLine { get; set; } = string.Empty;
    public string OutroLine { get; set; } = string.Empty;
}

public class TalentShowPresentationSettings
{
    public string WelcomeMessage { get; set; } = string.Empty;
    public string FooterMessage { get; set; } = string.Empty;
    public string IntermissionMessage { get; set; } = string.Empty;
    public string ThemeClass { get; set; } = string.Empty;
}

public class TalentShowRealtimeState
{
    public string ShowName { get; set; } = string.Empty;
    public string SessionCode { get; set; } = string.Empty;
    public bool IsLiveMode { get; set; }
    public int CurrentIndex { get; set; } = -1;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<TalentShowScheduleItem> Items { get; set; } = new();
    public TalentShowPresentationSettings Presentation { get; set; } = new();
}
