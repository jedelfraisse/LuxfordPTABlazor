using LuxfordPTAWeb.Shared.Enums;
using LuxfordPTAWeb.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace LuxfordPTAWeb.Services;

/// <summary>
/// Service for validating event category permissions and coordinator requirements
/// </summary>
public interface IEventPermissionService
{
    /// <summary>
    /// Checks if the current user can create or edit events in the specified category
    /// </summary>
    Task<bool> CanUserCreateEditEventAsync(ApplicationUser? user, EventCat eventCategory);
    
    /// <summary>
    /// Gets the coordinator requirement for the specified category
    /// </summary>
    Task<EventCoordinatorRequirement> GetCoordinatorRequirementAsync(int categoryId);
    
    /// <summary>
    /// Validates if the coordinator assignment is valid for the category
    /// </summary>
    Task<(bool IsValid, string? ErrorMessage)> ValidateCoordinatorAssignmentAsync(
        int categoryId, string? coordinatorId);
}

public class EventPermissionService : IEventPermissionService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventPermissionService> _logger;

    public EventPermissionService(
        UserManager<ApplicationUser> userManager,
        IServiceProvider serviceProvider,
        ILogger<EventPermissionService> logger)
    {
        _userManager = userManager;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<bool> CanUserCreateEditEventAsync(ApplicationUser? user, EventCat eventCategory)
    {
        if (user == null)
        {
            return false;
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        
        return eventCategory.EditingPermission switch
        {
            EventCategoryPermission.AdminOnly => userRoles.Contains("Admin"),
            EventCategoryPermission.BoardMembersAndAdmin => userRoles.Contains("Admin") || userRoles.Contains("BoardMember"),
            EventCategoryPermission.EventCoordinators => userRoles.Contains("Admin") || userRoles.Contains("BoardMember") || userRoles.Contains("EventCoordinator"),
            EventCategoryPermission.AllUsers => true, // All authenticated users can create events
            _ => false
        };
    }

    public async Task<EventCoordinatorRequirement> GetCoordinatorRequirementAsync(int categoryId)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
            
            var category = await db.EventCats.FindAsync(categoryId);
            return category?.CoordinatorRequirement ?? EventCoordinatorRequirement.Optional;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coordinator requirement for category {CategoryId}", categoryId);
            return EventCoordinatorRequirement.Optional; // Safe default
        }
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateCoordinatorAssignmentAsync(
        int categoryId, string? coordinatorId)
    {
        try
        {
            var requirement = await GetCoordinatorRequirementAsync(categoryId);

            return requirement switch
            {
                EventCoordinatorRequirement.NotNeeded => (true, null), // No validation needed
                EventCoordinatorRequirement.Optional => (true, null), // Always valid for optional
                EventCoordinatorRequirement.Required => string.IsNullOrEmpty(coordinatorId) 
                    ? (false, "A coordinator is required for events in this category") 
                    : (true, null),
                _ => (true, null) // Default to valid
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating coordinator assignment for category {CategoryId}", categoryId);
            return (false, "Error validating coordinator assignment");
        }
    }
}