using System;
using System.Collections.Generic;
using LuxfordPTAWeb.Shared.Enums;

namespace LuxfordPTAWeb.Shared.DTOs;

public class EventListItemDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string DescriptionMarkdown { get; set; } = string.Empty;
    public string DescriptionHtml { get; set; } = string.Empty;
    public string MoreDetailsMarkdown { get; set; } = string.Empty;
    public string MoreDetailsHtml { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? FlyerUrl { get; set; }
    public string? FlyerUrlsJson { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Planning;
    public DateTime? SetupStartTime { get; set; }
    public DateTime EventStartTime { get; set; }
    public DateTime EventEndTime { get; set; }
    public DateTime? CleanupEndTime { get; set; }
    public DateTime? SignupWindowStart { get; set; }
    public DateTime? SignupWindowEnd { get; set; }
    public bool RequiresVolunteers { get; set; }
    public bool RequiresSetup { get; set; }
    public bool RequiresCleanup { get; set; }
    public string? ExcelImportId { get; set; }
    public int SchoolYearId { get; set; }
    public string SchoolYearName { get; set; } = string.Empty;
    public SchoolYearStatus SchoolYearStatus { get; set; }
    public int EventCatId { get; set; }
    public string EventCatName { get; set; } = string.Empty;
    public string EventCatSlug { get; set; } = string.Empty;
    public string EventCatColorClass { get; set; } = string.Empty;
    public EventCoordinatorRequirement EventCatCoordinatorRequirement { get; set; } = EventCoordinatorRequirement.Optional;
    public int? EventSubTypeId { get; set; }
    public string EventSubCatName { get; set; } = string.Empty;
    public string? EventCoordinatorId { get; set; }
    public string EventCoordinatorFirstName { get; set; } = string.Empty;
    public string EventCoordinatorLastName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public string LastEditedBy { get; set; } = string.Empty;
    public DateTime LastEditedOn { get; set; }
    public bool HasPublicControlInformation { get; set; }
    public List<EventListDayDTO> EventDays { get; set; } = [];
}
