namespace LuxfordPTAWeb.Shared.Enums;

/// <summary>
/// Defines who can create or edit events within a specific category
/// </summary>
public enum EventCategoryPermission
{
    /// <summary>
    /// Only administrators can create/edit events in this category
    /// </summary>
    AdminOnly = 0,
    
    /// <summary>
    /// Board members and administrators can create/edit events in this category
    /// </summary>
    BoardMembersAndAdmin = 1,
    
    /// <summary>
    /// Event coordinators, board members, and administrators can create/edit events in this category
    /// </summary>
    EventCoordinators = 2,
    
    /// <summary>
    /// All authenticated users can create/edit events in this category
    /// </summary>
    AllUsers = 3
}