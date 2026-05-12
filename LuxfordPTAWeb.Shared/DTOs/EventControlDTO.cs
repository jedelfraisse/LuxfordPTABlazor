namespace LuxfordPTAWeb.Shared.DTOs;

public class EventControlDTO
{
    public int Id { get; set; }
    public string ControlType { get; set; } = string.Empty;
    public bool IsDirector { get; set; }
    public bool PublicInformation { get; set; }
    public string SettingsJson { get; set; } = "{}";
    public string DisplayName { get; set; } = string.Empty;
    public string NotesMarkdown { get; set; } = string.Empty;
    public string NotesHtml { get; set; } = string.Empty;
    public int SequenceOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
