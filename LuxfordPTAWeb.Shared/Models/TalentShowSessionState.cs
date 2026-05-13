namespace LuxfordPTAWeb.Shared.Models;

public class TalentShowSessionState
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = default!;
    public string SessionCode { get; set; } = string.Empty;
    public string ShowName { get; set; } = string.Empty;
    public string CurrentState { get; set; } = "PreShow";
    public string OverallState { get; set; } = "PreShow";
    public string LiveSubState { get; set; } = "HostTalk";
    public string NextLiveSubState { get; set; } = "ActShow";
    public int? CurrentActId { get; set; }
    public int? NextActId { get; set; }
    public int CurrentIndex { get; set; } = -1;
    public string HostScriptPointer { get; set; } = string.Empty;
    public DateTime? IntermissionEndTime { get; set; }
    public bool VotingOpen { get; set; }
    public DateTime SegmentStartedAtUtc { get; set; } = DateTime.UtcNow;
    public int EstimatedSegmentMinutes { get; set; } = 5;
    public bool IsLiveMode { get; set; }
    public string PresentationJson { get; set; } = "{}";
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
