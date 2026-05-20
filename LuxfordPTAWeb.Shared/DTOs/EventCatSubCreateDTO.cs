using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxfordPTAWeb.Shared.DTOs;

public class EventCatSubCreateDTO
{
	public int EventCatId { get; set; }
	public required string Name { get; set; }
	public int DisplayOrder { get; set; }
	public bool IsActive { get; set; }
	public required string Description { get; set; }
	public required string Icon { get; set; }
	public required string ColorClass { get; set; }
}
