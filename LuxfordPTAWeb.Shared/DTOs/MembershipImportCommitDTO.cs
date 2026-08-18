using LuxfordPTAWeb.Shared.Enums;

namespace LuxfordPTAWeb.Shared.DTOs;

/// <summary>Payload for finalizing an import after the admin has reviewed the preview.</summary>
public class MembershipImportCommitDTO
{
    public int SchoolYearId { get; set; }
    public List<MembershipImportRowDTO> Rows { get; set; } = [];
    public DuplicateImportStrategy DuplicateStrategy { get; set; } = DuplicateImportStrategy.Skip;
}
