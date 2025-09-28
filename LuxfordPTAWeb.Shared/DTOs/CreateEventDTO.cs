using System;
using System.ComponentModel.DataAnnotations;
using LuxfordPTAWeb.Shared.Enums;

namespace LuxfordPTAWeb.Shared.DTOs;

public class CreateEventDTO
{
    [Required]
    [StringLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required]
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

    [Required]
    public DateTime EventStartTime { get; set; }

    [Required]
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

    // Excel import tracking
    public string? ExcelImportId { get; set; }

    // Event copying and templates
    public string Slug { get; set; } = string.Empty;
    public int? SourceEventId { get; set; }
    public int CopyGeneration { get; set; } = 0;

    // Approval tracking
    public string ApprovalNotes { get; set; } = string.Empty;

    // Foreign Keys (Required)
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a school year")]
    public int SchoolYearId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Please select an event category")]
    public int EventCatId { get; set; }

    // Event Sub-type (Optional)
    public int? EventSubTypeId { get; set; }
}