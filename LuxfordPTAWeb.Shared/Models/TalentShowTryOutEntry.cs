namespace LuxfordPTAWeb.Shared.Models;

public static class TalentShowTryOutEntryStatus
{
    public const string Invited = "Invited";
    public const string StandBy = "StandBy";
    public const string OnDeck = "OnDeck";
    public const string Performing = "Performing";
    public const string Completed = "Completed";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";

    public static readonly string[] All =
    [
        Invited,
        StandBy,
        OnDeck,
        Performing,
        Completed,
        Approved,
        Rejected
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
    public string MusicUrl { get; set; } = string.Empty;
    public string PickupAdult { get; set; } = string.Empty;
    public DateTime? SlotTime { get; set; }
    public DateTime? CheckInTimestamp { get; set; }
    public string SessionLabel { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string ScoresJson { get; set; } = "[]";
    public string JudgeCompletionsJson { get; set; } = "[]";

    public bool Selected { get; set; }
    public string Status { get; set; } = TalentShowTryOutEntryStatus.Invited;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
