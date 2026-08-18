using LuxfordPTAWeb.Shared.DTOs;

namespace LuxfordPTAWeb.Services;

/// <summary>
/// Parses membership CSV uploads into a reviewable preview, then commits the reviewed
/// rows to the database. Parsing never writes to the database; only CommitAsync does.
/// </summary>
public interface IMembershipCsvImportService
{
    Task<MembershipImportPreviewDTO> ParsePreviewAsync(Stream csvStream, int schoolYearId);
    Task<MembershipImportResultDTO> CommitAsync(MembershipImportCommitDTO commit);
}
