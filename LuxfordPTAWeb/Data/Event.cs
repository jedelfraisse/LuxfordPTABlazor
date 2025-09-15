using System;
using System.Collections.Generic;
using LuxfordPTAWeb.Data.Enums;

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

		// Enhanced fields from Phase 1.1
		
		// Event Coordinator
		public string? EventCoordinatorId { get; set; }
		public ApplicationUser? EventCoordinator { get; set; }
		
		// Event Status
		public EventStatus Status { get; set; } = EventStatus.Planning;
		
		// Timing fields
		public DateTime? SetupStartTime { get; set; }
		public DateTime EventStartTime { get; set; }
		public DateTime EventEndTime { get; set; }
		public DateTime? CleanupEndTime { get; set; }
		
		// Capacity fields
		public int? MaxAttendees { get; set; }
		public int? EstimatedAttendees { get; set; }
		
		// Volunteer flags
		public bool RequiresVolunteers { get; set; } = false;
		public bool RequiresSetup { get; set; } = false;
		public bool RequiresCleanup { get; set; } = false;
		
		// Additional information fields
		public string Notes { get; set; } = string.Empty; // Internal notes for coordinators/admins
		public string PublicInstructions { get; set; } = string.Empty; // Public-facing instructions
		public string WeatherBackupPlan { get; set; } = string.Empty;
		
		// Excel import tracking (for volunteer coordination)
		public string? ExcelImportId { get; set; } // Links to volunteer signups from Excel imports

		// Existing relationships
		public int SchoolYearId { get; set; }
		public SchoolYear SchoolYear { get; set; } = default!;

		public ICollection<EventMainSponsor> MainSponsors { get; set; } = new List<EventMainSponsor>();
		public ICollection<EventOtherSponsor> OtherSponsors { get; set; } = new List<EventOtherSponsor>();

		public int EventTypeId { get; set; }
		public EventType EventType { get; set; } = default!;

		// New Event Sub-type fields
		public int? EventSubTypeId { get; set; }
		public EventSubType? EventSubType { get; set; }

		// Duration helpers (these ARE useful and used)
		public TimeSpan? EventDuration => EventEndTime != default && EventStartTime != default 
			? EventEndTime - EventStartTime 
			: null;
			
		public TimeSpan? TotalEventDuration => CleanupEndTime.HasValue && SetupStartTime.HasValue 
			? CleanupEndTime.Value - SetupStartTime.Value 
			: null;
	}
}
