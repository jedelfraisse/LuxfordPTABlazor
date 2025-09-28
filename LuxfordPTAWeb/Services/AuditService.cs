using LuxfordPTAWeb.Shared.Models;
using LuxfordPTAWeb.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace LuxfordPTAWeb.Services;

/// <summary>
/// Service for handling audit metadata on entities
/// Provides consistent audit tracking across the application
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Sets creation audit information
    /// </summary>
    /// <param name="entity">Entity implementing audit properties</param>
    /// <param name="user">User creating the entity</param>
    Task SetCreationAuditAsync<T>(T entity, ApplicationUser? user) where T : IAuditableEntity;
    
    /// <summary>
    /// Sets update audit information
    /// </summary>
    /// <param name="entity">Entity implementing audit properties</param>
    /// <param name="user">User updating the entity</param>
    /// <param name="changeNotes">Optional notes about the changes</param>
    Task SetUpdateAuditAsync<T>(T entity, ApplicationUser? user, string changeNotes = "") where T : IAuditableEntity;
    
    /// <summary>
    /// Gets the current user's ID for audit purposes
    /// </summary>
    /// <param name="user">Current user</param>
    /// <returns>User ID or "System" if no user</returns>
    string GetAuditUserId(ApplicationUser? user);
}

/// <summary>
/// Implementation of audit service
/// </summary>
public class AuditService : IAuditService
{
    private readonly ILogger<AuditService> _logger;

    public AuditService(ILogger<AuditService> logger)
    {
        _logger = logger;
    }

    public async Task SetCreationAuditAsync<T>(T entity, ApplicationUser? user) where T : IAuditableEntity
    {
        var now = DateTime.UtcNow;
        var userId = GetAuditUserId(user);
        
        entity.CreatedBy = userId;
        entity.CreatedOn = now;
        entity.LastEditedBy = userId;
        entity.LastEditedOn = now;
        entity.ChangeNotes = "Initial creation";
        
        _logger.LogInformation("Audit: Entity {EntityType} created by {UserId} at {CreatedOn}",
            typeof(T).Name, userId, now);
        
        await Task.CompletedTask;
    }

    public async Task SetUpdateAuditAsync<T>(T entity, ApplicationUser? user, string changeNotes = "") where T : IAuditableEntity
    {
        var now = DateTime.UtcNow;
        var userId = GetAuditUserId(user);
        
        entity.LastEditedBy = userId;
        entity.LastEditedOn = now;
        entity.ChangeNotes = string.IsNullOrWhiteSpace(changeNotes) ? "Updated" : changeNotes;
        
        _logger.LogInformation("Audit: Entity {EntityType} updated by {UserId} at {LastEditedOn}. Notes: {ChangeNotes}",
            typeof(T).Name, userId, now, entity.ChangeNotes);
        
        await Task.CompletedTask;
    }

    public string GetAuditUserId(ApplicationUser? user)
    {
        if (user?.Id != null)
        {
            // Use a combination of user ID and user name for better auditability
            var displayName = !string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName)
                ? $"{user.FirstName} {user.LastName}"
                : user.UserName ?? "Unknown User";
            return $"{user.Id}|{displayName}";
        }
        
        return "System";
    }
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