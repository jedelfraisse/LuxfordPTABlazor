namespace LuxfordPTAWeb.Shared.Models;

public class TalentShowAct
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = default!;
    public string PerformerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public int OrderIndex { get; set; }
    public string IntroLine { get; set; } = string.Empty;
    public string OutroLine { get; set; } = string.Empty;
    public string MediaFilePath { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool SelectedForShow { get; set; } = true;
    public string MusicUrl { get; set; } = string.Empty;
    public int MusicStartOffsetSeconds { get; set; }
    public string PerformerNotes { get; set; } = string.Empty;
    public string StageNotes { get; set; } = string.Empty;
    public string LightingNotes { get; set; } = string.Empty;
    public string SoundNotes { get; set; } = string.Empty;
    public string PropsRequired { get; set; } = string.Empty;
    public string HostIntro { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
