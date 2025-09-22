using System.Collections.Generic;

namespace LuxfordPTAWeb.Shared.Models;

public class Sponsor
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string LogoUrl { get; set; } = string.Empty;
	public string WebsiteUrl { get; set; } = string.Empty;
	
	public ICollection<SponsorAssignment> SponsorAssignments { get; set; } = new List<SponsorAssignment>();
	public ICollection<EventMainSponsor> MainEvents { get; set; } = new List<EventMainSponsor>();
	public ICollection<EventOtherSponsor> OtherEvents { get; set; } = new List<EventOtherSponsor>();
}