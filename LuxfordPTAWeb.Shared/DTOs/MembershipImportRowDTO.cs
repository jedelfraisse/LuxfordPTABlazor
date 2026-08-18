namespace LuxfordPTAWeb.Shared.DTOs;

/// <summary>One parsed CSV row, returned in a preview for admin review before commit.</summary>
public class MembershipImportRowDTO
{
    public int RowNumber { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime? JoinDate { get; set; }
    public string MemberType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public string? TeacherName { get; set; }

    /// <summary>True when Email already exists for this school year (in the DB or earlier in this same file).</summary>
    public bool IsDuplicate { get; set; }

    /// <summary>Validation problems (missing required field, bad date, etc.) — row cannot commit while non-empty.</summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>Whether the admin wants this row imported. Defaults to false for rows with errors.</summary>
    public bool Include { get; set; } = true;
}
