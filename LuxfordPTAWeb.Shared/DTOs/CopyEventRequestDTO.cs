namespace LuxfordPTAWeb.Shared.DTOs;

/// <summary>
/// DTO for copying events to different school years or dates
/// </summary>
public class CopyEventRequestDTO
{
    /// <summary>
    /// Optional new title for the copied event
    /// </summary>
    public string? NewTitle { get; set; }
    
    /// <summary>
    /// Optional new start date for the copied event
    /// </summary>
    public DateTime? NewStartDate { get; set; }
    
    /// <summary>
    /// Target school year ID for the copied event
    /// </summary>
    public int TargetSchoolYearId { get; set; }
    
    /// <summary>
    /// Optional new coordinator ID for the copied event
    /// </summary>
    public string? NewCoordinatorId { get; set; }
    
    /// <summary>
    /// Whether to copy multi-day event days along with the main event
    /// </summary>
    public bool CopyEventDays { get; set; } = true;
}