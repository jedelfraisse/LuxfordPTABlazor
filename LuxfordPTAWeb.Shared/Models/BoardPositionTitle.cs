using LuxfordPTAWeb.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace LuxfordPTAWeb.Shared.Models;

public class BoardPositionTitle
{
	public int Id { get; set; }
	
	[Required(ErrorMessage = "Position title is required")]
	[StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
	public string Title { get; set; } = string.Empty; // e.g., "President"
	
	public BoardRoleType RoleType { get; set; }
	public int SortOrder { get; set; }
	public bool IsRequired { get; set; } = false;
	public bool IsElected { get; set; } = false;
	public int? ElectionEventId { get; set; } // FK to Event, nullable
	
	[StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
	public string Description { get; set; } = string.Empty;
	
	public ICollection<BoardPosition> BoardPositions { get; set; } = new List<BoardPosition>();
}