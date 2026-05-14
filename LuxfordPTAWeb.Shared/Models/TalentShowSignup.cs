namespace LuxfordPTAWeb.Shared.Models;

public static class TalentShowSignupStatus
{
    public const string Pending = "Pending";
    public const string ApprovedDirect = "ApprovedDirect";
    public const string InvitedToTryOuts = "InvitedToTryOuts";
    public const string Rejected = "Rejected";
    public const string NeedsMoreInfo = "NeedsMoreInfo";

    public static readonly string[] All =
    [
        Pending,
        ApprovedDirect,
        InvitedToTryOuts,
        Rejected,
        NeedsMoreInfo
    ];
}

public class TalentShowSignup
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = default!;

    public string PerformerNames { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public string Teacher { get; set; } = string.Empty;
    public string ActTitle { get; set; } = string.Empty;
    public string ActDescription { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string SpecialRequirements { get; set; } = string.Empty;
    public string MediaUpload { get; set; } = string.Empty;

    public string Status { get; set; } = TalentShowSignupStatus.Pending;
    public string ReviewReason { get; set; } = string.Empty;
    public bool AllowResubmission { get; set; }
    public string TryOutSession { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
