using LuxfordPTAWeb.Shared.Enums;

namespace LuxfordPTAWeb.Shared.Models;

/// <summary>
/// A single PTA member for a specific school year. Internal-only (admin/board access) —
/// contains PII (name, email, phone) and must never be exposed on public endpoints.
/// </summary>
public class MembershipRecord
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }
    public string MemberType { get; set; } = string.Empty; // Parent/Guardian, Teacher/Staff, Student, etc.
    public int SchoolYearId { get; set; }
    public SchoolYear SchoolYear { get; set; } = null!;
    public decimal Price { get; set; }
    public MembershipStatus Status { get; set; } = MembershipStatus.Active;
    public string PaymentType { get; set; } = string.Empty; // cash, credit_card, etc.
    public string? TeacherName { get; set; }
    public bool IsStaff { get; set; } // derived from MemberType at import/save time

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
