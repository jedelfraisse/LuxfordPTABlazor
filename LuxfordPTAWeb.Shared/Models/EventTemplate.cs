namespace LuxfordPTAWeb.Shared.Models;

public class EventTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public int EventCatId { get; set; }
    public EventCat EventCat { get; set; } = default!;

    public int? EventSubTypeId { get; set; }
    public EventCatSub? EventCatSub { get; set; }
    public int? SourceEventId { get; set; }
    public Event? SourceEvent { get; set; }

    public string DefaultTitle { get; set; } = string.Empty;
    public string DefaultLocation { get; set; } = string.Empty;
    public string DefaultDescriptionMarkdown { get; set; } = string.Empty;
    public string DefaultMoreDetailsMarkdown { get; set; } = string.Empty;
    public bool RequiresVolunteers { get; set; } = false;
    public bool RequiresSetup { get; set; } = false;
    public bool RequiresCleanup { get; set; } = false;
}
