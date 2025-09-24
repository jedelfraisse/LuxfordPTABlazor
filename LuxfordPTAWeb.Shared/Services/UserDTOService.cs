using LuxfordPTAWeb.Shared.DTOs;
using LuxfordPTAWeb.Shared.Models;

namespace LuxfordPTAWeb.Shared.Services;

/// <summary>
/// Example service showing how to use the AssignedUserDTO pattern
/// for different scenarios where you need public-safe user information
/// </summary>
public static class UserDTOService
{
    /// <summary>
    /// Creates DTOs for public-facing API endpoints
    /// Example: GET /api/boardpositions/public/current
    /// </summary>
    public static List<AssignedUserDTO> CreatePublicBoardMemberList(IEnumerable<BoardPosition> positions)
    {
        return positions.ToAssignedUserDTOs();
    }

    /// <summary>
    /// Creates a single DTO for displaying individual board member information
    /// Example: Board member detail modal on home page
    /// </summary>
    public static AssignedUserDTO? CreatePublicBoardMemberInfo(BoardPosition position)
    {
        return position.ToAssignedUserDTO();
    }

    /// <summary>
    /// Example of filtering sensitive information for different user roles
    /// Admin users get full models, public users get DTOs
    /// </summary>
    public static object GetBoardMemberForRole(BoardPosition position, bool isPublicUser)
    {
        if (isPublicUser)
        {
            // Return DTO with only safe information
            return position.ToAssignedUserDTO() ?? new AssignedUserDTO();
        }
        else
        {
            // Return full model with all information for admin users
            return position;
        }
    }

    /// <summary>
    /// Example of creating DTOs for different contexts
    /// Home page vs. About page might show different levels of information
    /// </summary>
    public static AssignedUserDTO CreateContextualDTO(BoardPosition position, string context)
    {
        var dto = position.ToAssignedUserDTO();
        if (dto == null) return new AssignedUserDTO();

        // Customize DTO based on context
        switch (context.ToLower())
        {
            case "homepage":
                // Show basic info on home page
                return dto;
                
            case "aboutpage":
                // Could show more detailed bio information
                return dto;
                
            case "contactpage":
                // Might include role-specific contact info (but never personal contact)
                return dto;
                
            default:
                return dto;
        }
    }
}

/* 
USAGE EXAMPLES:

1. In a Controller (Public endpoint):
   [HttpGet("public/board-members")]
   public async Task<IEnumerable<AssignedUserDTO>> GetPublicBoardMembers()
   {
       var positions = await _db.BoardPositions.Include(bp => bp.AssignedUser)...
       return UserDTOService.CreatePublicBoardMemberList(positions);
   }

2. In a Blazor Component:
   var publicMembers = await Http.GetFromJsonAsync<List<AssignedUserDTO>>("api/boardpositions/public/current");

3. In a Service with Role-Based Access:
   var memberInfo = UserDTOService.GetBoardMemberForRole(position, !User.IsInRole("Admin"));

SECURITY BENEFITS:
- Prevents accidental exposure of sensitive user data
- Clear separation between public and private information
- Type-safe: compiler prevents accidentally returning full models to public endpoints  
- Easy to audit what information is being exposed publicly
- Consistent pattern for all public-facing user information

EXTENDING THE PATTERN:
- Create EventCoordinatorDTO for public event coordinator information
- Create VolunteerDTO for public volunteer information  
- Create SponsorContactDTO for public sponsor information
- Each DTO only includes fields appropriate for public viewing
*/