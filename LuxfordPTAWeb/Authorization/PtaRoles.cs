using System.Security.Claims;

namespace LuxfordPTAWeb.Authorization;

/// <summary>
/// Central place for the "is this an authenticated PTA member" check used to gate visibility of
/// not-yet-public content (upcoming school years, hidden milestones, etc.).
///
/// This is deliberately broader than [Authorize(Roles = "Admin,BoardMember")], which still governs
/// who can *edit* things — any of the app's real roles (Admin, BoardMember, Volunteer) is enough to
/// *see* hidden content; only anonymous visitors and authenticated-but-roleless accounts are held to
/// the public-only view.
/// </summary>
public static class PtaRoles
{
    public static readonly string[] All = ["Admin", "BoardMember", "Volunteer"];

    public static bool IsPtaMember(this ClaimsPrincipal user) =>
        All.Any(user.IsInRole);
}
