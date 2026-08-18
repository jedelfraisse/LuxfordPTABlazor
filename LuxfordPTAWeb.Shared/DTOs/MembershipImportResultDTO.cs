namespace LuxfordPTAWeb.Shared.DTOs;

/// <summary>Summary returned after committing an import.</summary>
public class MembershipImportResultDTO
{
    public int Imported { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = [];
}
