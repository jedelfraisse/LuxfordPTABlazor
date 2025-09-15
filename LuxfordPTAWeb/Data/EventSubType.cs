namespace LuxfordPTAWeb.Data;

public class EventSubType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // e.g., "Holiday", "Staff Development Day", "Early Dismissal"
    public string Slug { get; set; } = string.Empty; // e.g., "holiday", "staff-development", "early-dismissal"
    public string Description { get; set; } = string.Empty; // Detailed description of the subtype
    public int DisplayOrder { get; set; } = 0; // Order for displaying within parent EventType
    public bool IsActive { get; set; } = true; // Can be disabled instead of deleted
    public string Icon { get; set; } = string.Empty; // Bootstrap icon class (e.g., "bi-house", "bi-mortarboard")
    public string ColorClass { get; set; } = string.Empty; // CSS class for styling (e.g., "text-danger", "text-warning")
    
    // Parent relationship
    public int EventTypeId { get; set; }
    public EventType EventType { get; set; } = default!;
    
    // Navigation property for events that use this subtype
    public ICollection<Event> Events { get; set; } = new List<Event>();
    
    // Helper method to generate slug from name
    public static string GenerateSlug(string name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;
            
        return name.ToLowerInvariant()
                   .Replace(" ", "-")
                   .Replace("&", "and")
                   .Replace("/", "-")
                   .Replace("'", "")
                   .Replace("\"", "")
                   .Where(c => char.IsLetterOrDigit(c) || c == '-')
                   .Aggregate("", (current, c) => current + c);
    }
}
