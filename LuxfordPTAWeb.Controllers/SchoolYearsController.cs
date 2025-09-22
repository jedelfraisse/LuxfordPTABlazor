public class CreateSchoolYearDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string PrintableEventCalendar { get; set; } = string.Empty;
    public string ReflectionsTheme { get; set; } = string.Empty;
    public string DragonName { get; set; } = string.Empty;
    public string DragonPersona { get; set; } = string.Empty;
    public bool IsVisibleToPublic { get; set; } = false;
    public LuxfordPTAWeb.Data.Enums.SchoolYearStatus Status { get; set; } = LuxfordPTAWeb.Data.Enums.SchoolYearStatus.FutureYear;
}