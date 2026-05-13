namespace LuxfordPTAWeb.Shared.Models;

public class TalentShowDisplayAssignmentHistory
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = default!;
    public string DisplayCode { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnassignedAt { get; set; }
}
