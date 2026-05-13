namespace LuxfordPTAWeb.Shared.Models;

public class TalentShowVote
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = default!;
    public int? ActId { get; set; }
    public TalentShowAct? Act { get; set; }
    public string DeviceOrUserId { get; set; } = string.Empty;
    public string VoterName { get; set; } = string.Empty;
    public bool IsJudge { get; set; }
    public int TalentScore { get; set; } = 3;
    public int StagePresenceScore { get; set; } = 3;
    public int CreativityScore { get; set; } = 3;
    public int CrowdEngagementScore { get; set; } = 3;
    public int TotalScore { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
