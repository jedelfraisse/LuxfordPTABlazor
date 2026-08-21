using System.ComponentModel.DataAnnotations;

namespace LuxfordPTAWeb.Shared.Models;

/// <summary>
/// An admin-managed color choice offered in the Event Category / Sub-Category "Color Class"
/// dropdowns. Categories/sub-categories still just store a raw CSS class string (unchanged) — this
/// table only controls what shows up as selectable options, so the list can grow past the
/// originally-hardcoded 6 without a code change.
/// </summary>
public class EventColorOption
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty; // display label, e.g. "Sky Blue"

    [Required, MaxLength(50)]
    public string CssClass { get; set; } = string.Empty; // e.g. "text-info", or a custom class like "text-purple"

    public int DisplayOrder { get; set; }
}
