namespace LuxfordPTAWeb.Data;

public class EventType
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty; // e.g., "School Closed & Special Days"
	public string Slug { get; set; } = string.Empty; // e.g., "school-closed-days"
	public string Description { get; set; } = string.Empty; // e.g., "All holidays, staff days, and special schedule days for the school year."
	public bool IsMandatory { get; set; } // true for "School Closed", false for others
	public int DisplayOrder { get; set; } = 0; // Order for displaying in event categories
	public bool IsActive { get; set; } = true; // Can be disabled instead of deleted
	public EventTypeSize Size { get; set; } = EventTypeSize.Full; // Half or Full size card
	public string Icon { get; set; } = string.Empty; // Bootstrap icon class (e.g., "bi-calendar-x")
	public string ColorClass { get; set; } = string.Empty; // CSS class for styling (e.g., "text-danger")

	public ICollection<Event> Events { get; set; } = new List<Event>();
}

public enum EventTypeSize
{
	Half = 0,
	Full = 1
}
