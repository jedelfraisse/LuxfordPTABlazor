namespace LuxfordPTAWeb.Data;

public class SchoolYear
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty; // e.g., "2024-2025"
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }

	public ICollection<Event> Events { get; set; } = new List<Event>();
}
