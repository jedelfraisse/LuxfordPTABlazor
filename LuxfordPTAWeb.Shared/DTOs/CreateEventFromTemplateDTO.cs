namespace LuxfordPTAWeb.Shared.DTOs;

public class CreateEventFromTemplateDTO
{
    public int SchoolYearId { get; set; }
    public DateTime Date { get; set; }
    public string? TitleOverride { get; set; }
    public string? LocationOverride { get; set; }
    public string? DescriptionOverride { get; set; }
    public string? MoreDetailsOverride { get; set; }
}
