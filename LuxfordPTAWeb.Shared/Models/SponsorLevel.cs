using System.Collections.Generic;

namespace LuxfordPTAWeb.Shared.Models;

public class SponsorLevel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ColorCode { get; set; } = "#000";
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public ICollection<SponsorAssignment> SponsorAssignments { get; set; } = new List<SponsorAssignment>();
}

public class SponsorAssignment
{
    public int Id { get; set; }
    public int SponsorId { get; set; }
    public Sponsor Sponsor { get; set; } = null!;
    public int SponsorLevelId { get; set; }
    public SponsorLevel SponsorLevel { get; set; } = null!;
    public int SchoolYearId { get; set; }
    public SchoolYear SchoolYear { get; set; } = null!;
    public int? EventId { get; set; } // null for general/yearly sponsors
    public Event? Event { get; set; }
    // Optionally: public string Notes { get; set; }
}
