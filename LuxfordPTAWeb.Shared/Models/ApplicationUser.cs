using Microsoft.AspNetCore.Identity;

namespace LuxfordPTAWeb.Shared.Models;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public string FullName => $"{FirstName} {LastName}".Trim();
	public string Bio { get; set; } = string.Empty;
		
	// Contact Information
	public string Address { get; set; } = string.Empty;
	public string City { get; set; } = string.Empty;
	public string State { get; set; } = "VA"; // Default for Virginia
	public string ZipCode { get; set; } = string.Empty;
	
	// PTA-Specific
	public string PreferredName { get; set; } = string.Empty; // How they want to be addressed publicly
	public bool IsParent { get; set; } = true;
	public bool IsTeacher { get; set; } = false;
	public bool IsStaff { get; set; } = false;
	public bool IsSponsor { get; set; } = false; // Sponsor representative
	
	// Profile & Activity
	public string ProfilePictureUrl { get; set; } = string.Empty;
	public DateTime? JoinDate { get; set; }
	public bool IsActive { get; set; } = true;
	
	// Communication Preferences
	public bool ReceiveEmailNotifications { get; set; } = true;
	public bool ReceiveSmsNotifications { get; set; } = false;
	
	// Volunteer Information
	public string Skills { get; set; } = string.Empty; // Special skills for volunteering
	public string VolunteerInterests { get; set; } = string.Empty; // Areas they want to help with
	public bool CanVolunteerDuringSchoolHours { get; set; } = false;
	public bool CanVolunteerEvenings { get; set; } = true;
	public bool CanVolunteerWeekends { get; set; } = true;
	
	// Safety & Compliance
	public bool HasBackgroundCheck { get; set; } = false;
	public DateTime? BackgroundCheckExpiry { get; set; }
}
