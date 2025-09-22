namespace LuxfordPTAWeb.Shared.Enums;

public enum SchoolYearStatus
{
	FutureYear = 1,  // Upcoming years, board elected, planning phase
	PendingYear = 2,
	CurrentYear = 3, // Year in progress, active events
	Wrapup = 4,      // Audit/review phase, no new events
	PrevYear = 5     // Completed years, archived after audit
}
