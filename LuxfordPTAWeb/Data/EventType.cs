namespace LuxfordPTAWeb.Data;

public class EventType
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty; // e.g., "School Closed & Special Days"
	public string Description { get; set; } = string.Empty; // e.g., "All holidays, staff days, and special schedule days for the school year."
	public bool IsMandatory { get; set; } // true for "School Closed", false for others

	public ICollection<Event> Events { get; set; } = new List<Event>();

}
