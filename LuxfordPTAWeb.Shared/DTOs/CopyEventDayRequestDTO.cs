namespace LuxfordPTAWeb.Shared.DTOs;

/// <summary>
/// DTO for copying event days between events
/// </summary>
public class CopyEventDayRequestDTO
{
    /// <summary>
    /// Target event ID to copy the day to
    /// </summary>
    public int TargetEventId { get; set; }
    
    /// <summary>
    /// New day number for the copied day (0 = auto-assign)
    /// </summary>
    public int NewDayNumber { get; set; } = 0; // 0 = auto-assign
    
    /// <summary>
    /// New date for the copied day (null = use source date + 1 year)
    /// </summary>
    public DateTime? NewDate { get; set; } // null = use source date + 1 year
}