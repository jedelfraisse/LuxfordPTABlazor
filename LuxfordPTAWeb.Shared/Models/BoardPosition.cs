using LuxfordPTAWeb.Shared.Enums;

namespace LuxfordPTAWeb.Shared.Models;
public class BoardPosition
{
    public int Id { get; set; }
    public int BoardPositionTitleId { get; set; }
    public BoardPositionTitle BoardPositionTitle { get; set; } = null!;
    public bool IsVotingMember { get; set; }
    public int SchoolYearId { get; set; }
    public SchoolYear SchoolYear { get; set; } = null!;
    public string? UserId { get; set; } // FK to ApplicationUser, now nullable
    public ApplicationUser? AssignedUser { get; set; }
}
