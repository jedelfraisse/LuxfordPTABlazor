namespace LuxfordPTAWeb.Data.Enums;

public enum EventStatus
{
    Planning = 0,      // Event is being planned
	SubmittedForApproval = 1,
    Active = 2,        // Event is approved and visible to public
    InProgress = 3,    // Event is currently happening
    WrapUp = 4,
    Completed = 5,     // Event has finished
    Cancelled = 6      // Event has been cancelled
}