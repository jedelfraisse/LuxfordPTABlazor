namespace LuxfordPTAWeb.Client.Data
{
	public class Events
	{
		public string Title { get; set; } = string.Empty;
		public DateTime Date { get; set; }
		public string Description { get; set; } = string.Empty;
		public string Location { get; set; } = string.Empty;
		public string ImageUrl { get; set; } = string.Empty;
		public string Link { get; set; } = string.Empty;
		public Sponsors[] MainSponsors { get; set; } = Array.Empty<Sponsors>();
		public Sponsors[] OtherSponsors { get; set; } = Array.Empty<Sponsors>();
	}



}
