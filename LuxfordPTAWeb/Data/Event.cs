using System;
using System.Collections.Generic;

namespace LuxfordPTAWeb.Data
{
    public class Event
    {
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public DateTime Date { get; set; }
		public string Description { get; set; } = string.Empty;
		public string Location { get; set; } = string.Empty;
		public string ImageUrl { get; set; } = string.Empty;
		public string Link { get; set; } = string.Empty;

		public int SchoolYearId { get; set; }
		public SchoolYear SchoolYear { get; set; } = default!;

		public ICollection<EventMainSponsor> MainSponsors { get; set; } = new List<EventMainSponsor>();
		public ICollection<EventOtherSponsor> OtherSponsors { get; set; } = new List<EventOtherSponsor>();

		public int EventTypeId { get; set; }
		public EventType EventType { get; set; } = default!;
	}
}
