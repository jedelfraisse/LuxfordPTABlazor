namespace LuxfordPTAWeb.Shared.Enums;

/// <summary>
/// Defines whether an event coordinator is required for events in a specific category
/// </summary>
public enum EventCoordinatorRequirement
{
    /// <summary>
    /// No coordinator field is shown - events in this category don't need coordinators
    /// </summary>
    NotNeeded = 0,
    
    /// <summary>
    /// Coordinator field is shown but not required - events can have coordinators but it's optional
    /// </summary>
    Optional = 1,
    
    /// <summary>
    /// Coordinator field is shown and required - all events in this category must have a coordinator
    /// </summary>
    Required = 2
}