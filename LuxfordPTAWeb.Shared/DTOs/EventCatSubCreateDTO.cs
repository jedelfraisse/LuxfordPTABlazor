using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxfordPTAWeb.Shared.DTOs;

public class EventCatSubCreateDTO
{
	public int EventCatId { get; set; }
	public string Name { get; set; }
	public int DisplayOrder { get; set; }
	public bool IsActive { get; set; }
	public string Description { get; set; } // Added for controller compatibility
	public string Icon { get; set; } // Added for controller compatibility
	public string ColorClass { get; set; } // Added for controller compatibility
}
