namespace LuxfordPTAWeb.Shared.Models;


public class SchoolYear
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty; // e.g., "2024-2025"
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }	public string PrintableEventCalendar { get; set; } = string.Empty;
	public ICollection<Event> Events { get; set; } = new List<Event>();
	public ICollection<SponsorAssignment> SponsorAssignments { get; set; } = new List<SponsorAssignment>();
	public ICollection<BoardPosition> BoardPositions { get; set; } = new List<BoardPosition>();

	// Dragon Theme Information / From Reflections announcmented during May.
	public string ReflectionsTheme { get; set; } = string.Empty;
	
	// Add this property for visibility control
	public bool IsVisibleToPublic { get; set; } = false;

    // Or use your existing status enum
    public LuxfordPTAWeb.Shared.Enums.SchoolYearStatus Status { get; set; } = LuxfordPTAWeb.Shared.Enums.SchoolYearStatus.FutureYear;
}
