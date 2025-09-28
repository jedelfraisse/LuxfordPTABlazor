namespace LuxfordPTAWeb.Shared.DTOs;

/// <summary>
/// DTO for overriding board position user assignments
/// </summary>
public class AssignUserOverrideDTO
{
    /// <summary>
    /// Board position title ID to assign user to
    /// </summary>
    public int BoardPositionTitleId { get; set; }
    
    /// <summary>
    /// User ID to assign (null to unassign)
    /// </summary>
    public string? UserId { get; set; }
    
    /// <summary>
    /// School year ID for the assignment
    /// </summary>
    public int SchoolYearId { get; set; }
}