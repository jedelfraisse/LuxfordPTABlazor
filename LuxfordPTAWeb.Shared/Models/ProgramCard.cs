using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxfordPTAWeb.Shared.Models;

public class ProgramCard
{
	public int Id { get; set; }
	
	[Required]
	[MaxLength(200)]
	public string ProgramTitle { get; set; } = string.Empty;
	
	[MaxLength(500)]
	public string ShortDescription { get; set; } = string.Empty;
	
	// Long description for detail pages - Markdown format
	[Column(TypeName = "nvarchar(max)")]
	public string? LongDescription { get; set; }
	
	[MaxLength(500)]
	public string Link { get; set; } = string.Empty;
	
	[MaxLength(500)]
	public string ImageUrl { get; set; } = string.Empty;
	
	[MaxLength(500)]
	public string? FlyerUrl { get; set; }
	
	/// <summary>
	/// Internal flag for routing:
	/// - true = Uses dynamic ProgramDetail.razor page (renders LongDescription Markdown)
	/// - false = Has its own dedicated custom Razor page
	/// </summary>
	public bool IsHybrid { get; set; } = false;
	
	// Optional: Add school year relationship if programs change by year
	// public int? SchoolYearId { get; set; }
	// public SchoolYear? SchoolYear { get; set; }
	
	public int DisplayIndex { get; set; } = 0;
	
	public bool IsActive { get; set; } = true;
}

