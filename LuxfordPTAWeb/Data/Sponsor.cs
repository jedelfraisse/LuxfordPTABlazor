using System.Collections.Generic;

namespace LuxfordPTAWeb.Data
{
	public class Sponsor
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string LogoUrl { get; set; } = string.Empty;
		public string WebsiteUrl { get; set; } = string.Empty;

		public ICollection<EventMainSponsor> MainEvents { get; set; } = new List<EventMainSponsor>();
		public ICollection<EventOtherSponsor> OtherEvents { get; set; } = new List<EventOtherSponsor>();
	}
}