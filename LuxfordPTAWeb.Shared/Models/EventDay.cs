using System;
using System.Collections.Generic;

namespace LuxfordPTAWeb.Shared.Models;

public class EventDay
{
    public int Id { get; set; }
    
    // Parent event relationship
    public int EventId { get; set; }
    public Event Event { get; set; } = default!;
    
    // Day information
    public int DayNumber { get; set; } // 1, 2, 3, etc.
    public DateTime Date { get; set; }
    public string DayTitle { get; set; } = string.Empty; // e.g., "Kick-off Day", "Fire Safety Demo"
    public string Description { get; set; } = string.Empty; // Day-specific activities description
    public string Location { get; set; } = string.Empty; // Can be different per day
    
    // Timing (can override parent event times)
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    
    // Day management
    public bool IsActive { get; set; } = true; // Can disable specific days
    public string SpecialInstructions { get; set; } = string.Empty; // Day-specific instructions
    
    // Capacity (day-specific)
    public int? MaxAttendees { get; set; }
    public int? EstimatedAttendees { get; set; }
    
    // Weather and logistics
    public string WeatherBackupPlan { get; set; } = string.Empty; // Day-specific backup plan
    
    // Navigation properties for related entities (Phase 2 - commented out for now)
    // public ICollection<EventStation> Stations { get; set; } = new List<EventStation>();
    // public ICollection<EventScheduleItem> ScheduleItems { get; set; } = new List<EventScheduleItem>();
    // public ICollection<EventVolunteerRole> VolunteerRoles { get; set; } = new List<EventVolunteerRole>();
    // public ICollection<EventRule> Rules { get; set; } = new List<EventRule>();
    // public ICollection<EventDocument> Documents { get; set; } = new List<EventDocument>();
    
    // Calculated properties
    public TimeSpan? Duration => StartTime.HasValue && EndTime.HasValue
        ? EndTime.Value - StartTime.Value
        : null;
        
    // Helper to get effective start/end times (fallback to parent event if not set)
    public DateTime EffectiveStartTime => StartTime ?? Event?.EventStartTime ?? Date;
    public DateTime EffectiveEndTime => EndTime ?? Event?.EventEndTime ?? Date.AddHours(2);
    
    public string EffectiveLocation => !string.IsNullOrEmpty(Location) ? Location : Event?.Location ?? string.Empty;
}