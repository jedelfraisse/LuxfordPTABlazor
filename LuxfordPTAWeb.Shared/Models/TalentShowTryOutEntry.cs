namespace LuxfordPTAWeb.Shared.Models;

public static class TalentShowTryOutEntryStatus
{
    public const string Scheduled = "Scheduled";
    public const string Arrived = "Arrived";
    public const string Performing = "Performing";
    public const string Completed = "Completed";
    public const string NoShow = "NoShow";

    public static readonly string[] All =
    [
        Scheduled,
        Arrived,
        Performing,
        Completed,
        NoShow
    ];
}

public class TalentShowTryOutEntry
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = default!;

    public int? SignupId { get; set; }
    public TalentShowSignup? Signup { get; set; }
    public int? ActId { get; set; }

    public string PerformerNames { get; set; } = string.Empty;
    public string ActTitle { get; set; } = string.Empty;
    public DateTime? SlotTime { get; set; }
    public string SessionLabel { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public bool Selected { get; set; }
    public string Status { get; set; } = TalentShowTryOutEntryStatus.Scheduled;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
