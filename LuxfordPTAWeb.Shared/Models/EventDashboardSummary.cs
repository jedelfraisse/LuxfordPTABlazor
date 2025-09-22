namespace LuxfordPTAWeb.Shared.Models;

public class EventDashboardSummary
{
    public int TotalEvents { get; set; }
    public int PendingApproval { get; set; }
    public int ActiveEvents { get; set; }
    public int InProgressEvents { get; set; }
    public int CompletedEvents { get; set; }
    public int CancelledEvents { get; set; }
    public int PlanningEvents { get; set; }
    public int UpcomingNext7Days { get; set; }
    public int UpcomingNext30Days { get; set; }
    public int NeedsAttention { get; set; }
    public int RequiringVolunteers { get; set; }
}