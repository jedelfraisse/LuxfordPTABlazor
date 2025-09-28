namespace LuxfordPTAWeb.Shared.Interfaces;

/// <summary>
/// Interface for entities that support audit tracking
/// </summary>
public interface IAuditableEntity
{
    string CreatedBy { get; set; }
    DateTime CreatedOn { get; set; }
    string LastEditedBy { get; set; }
    DateTime LastEditedOn { get; set; }
    string ChangeNotes { get; set; }
}

/// <summary>
/// Extension methods for working with audit information
/// </summary>
public static class AuditExtensions
{
    /// <summary>
    /// Extracts the user display name from the audit user ID
    /// </summary>
    /// <param name="auditUserId">Audit user ID in format "userId|displayName"</param>
    /// <returns>Display name or the original string if format is unexpected</returns>
    public static string GetDisplayName(this string auditUserId)
    {
        if (string.IsNullOrWhiteSpace(auditUserId))
            return "Unknown";
            
        if (auditUserId == "System")
            return "System";
            
        var parts = auditUserId.Split('|');
        return parts.Length > 1 ? parts[1] : auditUserId;
    }
    
    /// <summary>
    /// Extracts the user ID from the audit user ID
    /// </summary>
    /// <param name="auditUserId">Audit user ID in format "userId|displayName"</param>
    /// <returns>User ID or null if format is unexpected</returns>
    public static string? GetUserId(this string auditUserId)
    {
        if (string.IsNullOrWhiteSpace(auditUserId) || auditUserId == "System")
            return null;
            
        var parts = auditUserId.Split('|');
        return parts.Length > 0 ? parts[0] : null;
    }
}