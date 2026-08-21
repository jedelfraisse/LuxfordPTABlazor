using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class EventColorOptionsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public EventColorOptionsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<EventColorOption>> Get()
    {
        return await _db.EventColorOptions
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<ActionResult<EventColorOption>> Post([FromBody] EventColorOption dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.CssClass))
            return BadRequest("Name and CSS class are both required.");

        var nextOrder = await _db.EventColorOptions.AnyAsync()
            ? await _db.EventColorOptions.MaxAsync(c => c.DisplayOrder) + 1
            : 1;

        var color = new EventColorOption
        {
            Name = dto.Name.Trim(),
            CssClass = dto.CssClass.Trim(),
            DisplayOrder = dto.DisplayOrder > 0 ? dto.DisplayOrder : nextOrder
        };

        _db.EventColorOptions.Add(color);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), color);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Put(int id, [FromBody] EventColorOption dto)
    {
        var color = await _db.EventColorOptions.FindAsync(id);
        if (color == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.CssClass))
            return BadRequest("Name and CSS class are both required.");

        color.Name = dto.Name.Trim();
        color.CssClass = dto.CssClass.Trim();
        color.DisplayOrder = dto.DisplayOrder;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Delete(int id)
    {
        var color = await _db.EventColorOptions.FindAsync(id);
        if (color == null) return NotFound();

        // Categories/sub-categories store the CSS class as a plain string, not a FK, so deleting a
        // color option here never breaks existing assignments — it just stops offering it going
        // forward. No "in use" guard needed.
        _db.EventColorOptions.Remove(color);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
