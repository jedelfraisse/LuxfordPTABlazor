namespace LuxfordPTAWeb.Shared.Models;

public class EventOtherSponsor
{
	public int EventId { get; set; }
	public Event Event { get; set; } = default!;
	public int SponsorId { get; set; }
	public Sponsor Sponsor { get; set; } = default!;
}