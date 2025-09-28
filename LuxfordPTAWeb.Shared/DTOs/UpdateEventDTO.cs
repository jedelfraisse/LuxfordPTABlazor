using System;
using LuxfordPTAWeb.Shared.Enums;

namespace LuxfordPTAWeb.Shared.DTOs;

/// <summary>
/// DTO for updating events that includes audit information
/// </summary>
public class UpdateEventDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    
    // Event Coordinator
    public string? EventCoordinatorId { get; set; }
    
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
    public string Notes { get; set; } = string.Empty;
    public string PublicInstructions { get; set; } = string.Empty;
    public string WeatherBackupPlan { get; set; } = string.Empty;
    public string? ExcelImportId { get; set; }
    
    // Relationships
    public int SchoolYearId { get; set; }
    public int EventCatId { get; set; }
    public int? EventSubTypeId { get; set; }
    
    // NEW: Audit information for the update
    public string ChangeNotes { get; set; } = string.Empty; // Optional notes about what was changed
}