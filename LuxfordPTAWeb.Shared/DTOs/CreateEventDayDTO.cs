using System;
using System.ComponentModel.DataAnnotations;

namespace LuxfordPTAWeb.Shared.DTOs;

public class CreateEventDayDTO
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "EventId is required")]
    public int EventId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "DayNumber must be at least 1")]
    public int DayNumber { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [StringLength(255)]
    public string DayTitle { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [StringLength(500)]
    public string Location { get; set; } = string.Empty;

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public bool IsActive { get; set; } = true;

    public string SpecialInstructions { get; set; } = string.Empty;

    public int? MaxAttendees { get; set; }

    public int? EstimatedAttendees { get; set; }

    public string WeatherBackupPlan { get; set; } = string.Empty;
}