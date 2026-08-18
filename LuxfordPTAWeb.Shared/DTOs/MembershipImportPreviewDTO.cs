namespace LuxfordPTAWeb.Shared.DTOs;

/// <summary>Result of parsing an uploaded CSV, before anything is written to the database.</summary>
public class MembershipImportPreviewDTO
{
    public int SchoolYearId { get; set; }
    public List<MembershipImportRowDTO> Rows { get; set; } = [];
    public int TotalRows => Rows.Count;
    public int ValidRows => Rows.Count(r => r.Errors.Count == 0);
    public int ErrorRows => Rows.Count(r => r.Errors.Count > 0);
    public int DuplicateRows => Rows.Count(r => r.IsDuplicate);

    /// <summary>File/header-level problems that prevented parsing (e.g. missing required column).</summary>
    public List<string> FileErrors { get; set; } = [];
}
