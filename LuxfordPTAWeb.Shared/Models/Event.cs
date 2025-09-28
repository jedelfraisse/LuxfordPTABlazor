using System;
using System.Collections.Generic;
using LuxfordPTAWeb.Shared.Enums;

namespace LuxfordPTAWeb.Shared.Models;

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

	
	// NEW: Event copying and templates
	public string Slug { get; set; } = string.Empty; // URL-friendly identifier
	public int? SourceEventId { get; set; } // Reference to source event if this is a copy
	public Event? SourceEvent { get; set; }
	public int CopyGeneration { get; set; } = 0; // 0 = original, 1 = first copy, etc.
	
	// NEW: Approval tracking
	public string? ApprovedByUserId { get; set; }
	public ApplicationUser? ApprovedBy { get; set; }
	public DateTime? ApprovedDate { get; set; }
	public string ApprovalNotes { get; set; } = string.Empty;

	// Existing relationships
	public int SchoolYearId { get; set; }
	public SchoolYear SchoolYear { get; set; } = default!;

	public ICollection<EventMainSponsor> MainSponsors { get; set; } = new List<EventMainSponsor>();
	public ICollection<EventOtherSponsor> OtherSponsors { get; set; } = new List<EventOtherSponsor>();

	public int EventCatId { get; set; }
	public EventCat EventCat { get; set; } = default!;

	// Event Sub-type fields
	public int? EventSubTypeId { get; set; }
	public EventCatSub? EventCatSub { get; set; }
	
	// NEW: Multi-day event days
	public ICollection<EventDay> EventDays { get; set; } = new List<EventDay>();
	
	// NEW: Events copied from this event
	public ICollection<Event> CopiedEvents { get; set; } = new List<Event>();

	// Duration helpers (these ARE useful and used)
	public TimeSpan? EventDuration => EventEndTime != default && EventStartTime != default
		? EventEndTime - EventStartTime
		: null;

	public TimeSpan? TotalEventDuration => CleanupEndTime.HasValue && SetupStartTime.HasValue
		? CleanupEndTime.Value - SetupStartTime.Value
		: null;
		
	// NEW: Multi-day event helpers
	public DateTime? MultiDayStartDate => EventDays.Count > 1 && EventDays.Any()
		? EventDays.OrderBy(d => d.Date).First().Date
		: Date;

	public DateTime? MultiDayEndDate => EventDays.Count > 1 && EventDays.Any()
		? EventDays.OrderByDescending(d => d.Date).First().Date
		: Date;

	public int DayCount => EventDays.Count > 1 ? EventDays.Count : 1;
	
	// Helper method to generate slug from title
	public static string GenerateSlug(string title)
	{
		if (string.IsNullOrEmpty(title))
			return string.Empty;
			
		return title.ToLowerInvariant()
				   .Replace(" ", "-")
				   .Replace("&", "and")
				   .Replace("/", "-")
				   .Replace("'", "")
				   .Replace("\"", "")
				   .Where(c => char.IsLetterOrDigit(c) || c == '-')
				   .Aggregate("", (current, c) => current + c);
	}

	// Flexible sponsor/event/level relationships
	public ICollection<SponsorAssignment> SponsorAssignments { get; set; } = new List<SponsorAssignment>();
}
