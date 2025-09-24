using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuxfordPTAWeb.Shared.Models;

namespace LuxfordPTAWeb.Shared.DTOs;

/// <summary>
/// Public-safe DTO for displaying board position assigned users.
/// Only includes information appropriate for public viewing on the home page.
/// Excludes private information like address, phone, email, etc.
/// </summary>
public class AssignedUserDTO
{
    /// <summary>
    /// User's full name for public display
    /// </summary>
    public string? FullName { get; set; }
    
    /// <summary>
    /// User's preferred name if they want to be addressed differently in public
    /// Falls back to FullName if PreferredName is empty
    /// </summary>
    public string? PreferredName { get; set; }
    
    /// <summary>
    /// Public bio information that the user has chosen to share
    /// </summary>
    public string? Bio { get; set; }
    
    /// <summary>
    /// Board position title (e.g., "President", "Vice President")
    /// </summary>
    public string? RoleTitle { get; set; }
    
    /// <summary>
    /// Description of the board position duties (for modal display)
    /// </summary>
    public string? RoleDescription { get; set; }
    
    /// <summary>
    /// Whether this is a voting member of the board
    /// </summary>
    public bool IsVotingMember { get; set; }
    
    /// <summary>
    /// School year label (e.g., "2024-2025")
    /// </summary>
    public string? SchoolYearLabel { get; set; }
    
    /// <summary>
    /// Sort order for displaying board members
    /// </summary>
    public int SortOrder { get; set; }
    
    /// <summary>
    /// Profile picture URL for public display (if available)
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    // PTA Community Role Information (public display)
    /// <summary>
    /// Whether the board member is a parent/guardian
    /// </summary>
    public bool IsParent { get; set; }
    
    /// <summary>
    /// Whether the board member is a teacher at the school
    /// </summary>
    public bool IsTeacher { get; set; }
    
    /// <summary>
    /// Whether the board member is school staff
    /// </summary>
    public bool IsStaff { get; set; }
    
    /// <summary>
    /// Whether the board member is a sponsor representative
    /// </summary>
    public bool IsSponsor { get; set; }

    /// <summary>
    /// Gets the display name, preferring PreferredName over FullName
    /// </summary>
    public string DisplayName => !string.IsNullOrWhiteSpace(PreferredName) ? PreferredName : FullName ?? "Unknown";
    
    /// <summary>
    /// Whether this board member has additional details to show (bio or role description)
    /// </summary>
    public bool HasAdditionalDetails => !string.IsNullOrWhiteSpace(Bio) || !string.IsNullOrWhiteSpace(RoleDescription);
    
    /// <summary>
    /// Gets a list of community roles for display (e.g., "Parent, Sponsor Representative")
    /// </summary>
    public string CommunityRoles
    {
        get
        {
            var roles = new List<string>();
            
            if (IsParent) roles.Add("Parent/Guardian");
            if (IsTeacher) roles.Add("Teacher");
            if (IsStaff) roles.Add("School Staff");
            if (IsSponsor) roles.Add("Sponsor Representative");
            
            return roles.Any() ? string.Join(", ", roles) : "Community Member";
        }
    }
}

/// <summary>
/// Extension methods for converting from full models to public DTOs
/// </summary>
public static class AssignedUserDTOExtensions
{
    /// <summary>
    /// Converts a BoardPosition with its related entities to a public-safe AssignedUserDTO
    /// </summary>
    public static AssignedUserDTO? ToAssignedUserDTO(this BoardPosition boardPosition)
    {
        if (boardPosition.AssignedUser == null)
            return null;

        return new AssignedUserDTO
        {
            FullName = boardPosition.AssignedUser.FullName,
            PreferredName = !string.IsNullOrWhiteSpace(boardPosition.AssignedUser.PreferredName) 
                ? boardPosition.AssignedUser.PreferredName 
                : null,
            Bio = !string.IsNullOrWhiteSpace(boardPosition.AssignedUser.Bio) 
                ? boardPosition.AssignedUser.Bio 
                : null,
            RoleTitle = boardPosition.BoardPositionTitle?.Title,
            RoleDescription = boardPosition.BoardPositionTitle?.Description,
            IsVotingMember = boardPosition.IsVotingMember,
            SchoolYearLabel = boardPosition.SchoolYear?.Name,
            SortOrder = boardPosition.BoardPositionTitle?.SortOrder ?? 0,
            ProfilePictureUrl = !string.IsNullOrWhiteSpace(boardPosition.AssignedUser.ProfilePictureUrl) 
                ? boardPosition.AssignedUser.ProfilePictureUrl 
                : null,
            // Include public community role information
            IsParent = boardPosition.AssignedUser.IsParent,
            IsTeacher = boardPosition.AssignedUser.IsTeacher,
            IsStaff = boardPosition.AssignedUser.IsStaff,
            IsSponsor = boardPosition.AssignedUser.IsSponsor
        };
    }

    /// <summary>
    /// Converts a collection of BoardPositions to AssignedUserDTOs, filtering out unassigned positions
    /// </summary>
    public static List<AssignedUserDTO> ToAssignedUserDTOs(this IEnumerable<BoardPosition> boardPositions)
    {
        return boardPositions
            .Where(bp => bp.AssignedUser != null)
            .Select(bp => bp.ToAssignedUserDTO())
            .Where(dto => dto != null)
            .Cast<AssignedUserDTO>()
            .OrderBy(dto => dto.SortOrder)
            .ThenBy(dto => dto.RoleTitle)
            .ToList();
    }
}

/* 
PRIVATE INFORMATION EXCLUDED FROM PUBLIC DTO:
- Email addresses (User.Email)
- Phone numbers (User.PhoneNumber)
- Physical address (Address, City, State, ZipCode)
- Join date (JoinDate)
- Volunteer preferences and availability
- Background check status
- Communication preferences
- Skills and volunteer interests
- Any sensitive Identity information

PUBLIC COMMUNITY INFORMATION INCLUDED:
- Parent/Guardian status
- Teacher status  
- School Staff status
- Sponsor Representative status
- Public bio information
- Preferred display name
- Board position information

ADMIN-ONLY INFORMATION (use full ApplicationUser model):
- All contact information
- Volunteer history and preferences
- Background check status and expiry dates
- Communication preferences
- Detailed volunteer skills and interests
- Role assignment history
- Login and security information
*/
