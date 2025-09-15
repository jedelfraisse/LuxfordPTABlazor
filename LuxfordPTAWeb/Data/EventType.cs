namespace LuxfordPTAWeb.Data;

public class EventType
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty; // e.g., "School Closed & Special Days"
	public string Slug { get; set; } = string.Empty; // e.g., "school-closed-days"
	public string Description { get; set; } = string.Empty; // e.g., "All holidays, staff days, and special schedule days for the school year."
	public int DisplayOrder { get; set; } = 0; // Order for displaying in event categories
	public bool IsActive { get; set; } = true; // Can be disabled instead of deleted
	public EventTypeSize Size { get; set; } = EventTypeSize.Full; // Half or Full size card
	public string Icon { get; set; } = string.Empty; // Bootstrap icon class (e.g., "bi-calendar-x")
	public string ColorClass { get; set; } = string.Empty; // CSS class for styling (e.g., "text-danger")

	// New display options for main events page
	public EventDisplayMode DisplayMode { get; set; } = EventDisplayMode.ShowCategory; // How to display this type
	public int MaxEventsToShow { get; set; } = 0; // For InlineWithLimit mode (0 = unlimited)
	public bool ShowViewEventsButton { get; set; } = true; // Whether to show "View Events" button
	public bool ShowInlineOnMainPage { get; set; } = false; // Show events directly on main page instead of category card

	public ICollection<Event> Events { get; set; } = new List<Event>();
}

public enum EventTypeSize
{
	Half = 0,
	Full = 1
}

public enum EventDisplayMode
{
	ShowCategory = 0,     // Show normal category card with "View Events" button
	HideEvents = 1,       // Don't show any events or category
	InlineAll = 2,        // Show all events directly on main page in a section
	InlineWithLimit = 3   // Show limited number of events directly on main page
}
