namespace LuxfordPTAWeb.Shared.DTOs;

/// <summary>
/// DTO containing event statistics for dashboard display
/// </summary>
public class EventDashboardSummaryDTO
{
    /// <summary>
    /// Total number of events in the selected scope
    /// </summary>
    public int TotalEvents { get; set; }
    
    /// <summary>
    /// Number of events pending approval
    /// </summary>
    public int PendingApproval { get; set; }
    
    /// <summary>
    /// Number of active events
    /// </summary>
    public int ActiveEvents { get; set; }
    
    /// <summary>
    /// Number of events currently in progress
    /// </summary>
    public int InProgressEvents { get; set; }
    
    /// <summary>
    /// Number of completed events
    /// </summary>
    public int CompletedEvents { get; set; }
    
    /// <summary>
    /// Number of cancelled events
    /// </summary>
    public int CancelledEvents { get; set; }
    
    /// <summary>
    /// Number of events still in planning phase
    /// </summary>
    public int PlanningEvents { get; set; }
    
    /// <summary>
    /// Number of upcoming events in the next 7 days
    /// </summary>
    public int UpcomingNext7Days { get; set; }
    
    /// <summary>
    /// Number of upcoming events in the next 30 days
    /// </summary>
    public int UpcomingNext30Days { get; set; }
    
    /// <summary>
    /// Number of events that need attention (pending approval, overdue, etc.)
    /// </summary>
    public int NeedsAttention { get; set; }
    
    /// <summary>
    /// Number of events that require volunteers
    /// </summary>
    public int RequiringVolunteers { get; set; }
}