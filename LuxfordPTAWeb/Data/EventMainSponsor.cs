namespace LuxfordPTAWeb.Data
{
	public class EventMainSponsor
	{
		public int EventId { get; set; }
		public Event Event { get; set; } = default!;
		public int SponsorId { get; set; }
		public Sponsor Sponsor { get; set; } = default!;
	}
}