using LuxfordPTAWeb.Shared.Enums;

namespace LuxfordPTAWeb.Shared.DTOs;

/// <summary>Create/edit payload for a single membership record.</summary>
public class MembershipRecordDTO
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; } = DateTime.Today;
    public string MemberType { get; set; } = string.Empty;
    public int SchoolYearId { get; set; }
    public decimal Price { get; set; }
    public MembershipStatus Status { get; set; } = MembershipStatus.Active;
    public string PaymentType { get; set; } = string.Empty;
    public string? TeacherName { get; set; }
}
