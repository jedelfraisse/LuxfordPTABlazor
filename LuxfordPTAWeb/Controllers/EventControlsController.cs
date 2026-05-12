using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Shared.DTOs;
using LuxfordPTAWeb.Shared.Models;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class EventControlsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public EventControlsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<ActionResult<EventControl>> Get(int id)
    {
        var control = await _db.EventControls.FirstOrDefaultAsync(ec => ec.Id == id);
        if (control == null)
        {
            return NotFound();
        }

        return Ok(control);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Put(int id, [FromBody] EventControlDTO updatedControl)
    {
        var control = await _db.EventControls.FirstOrDefaultAsync(ec => ec.Id == id);
        if (control == null)
        {
            return NotFound();
        }

        var eventItem = await _db.Events
            .Include(e => e.EventControls)
            .FirstOrDefaultAsync(e => e.Id == control.EventId);
        if (eventItem == null)
        {
            return NotFound();
        }

        if (updatedControl.IsDirector)
        {
            foreach (var otherControl in eventItem.EventControls.Where(ec => ec.Id != control.Id))
            {
                otherControl.IsDirector = false;
            }
        }

        control.ControlType = updatedControl.ControlType.Trim();
        control.IsDirector = updatedControl.IsDirector;
        control.PublicInformation = updatedControl.PublicInformation;
        control.SettingsJson = string.IsNullOrWhiteSpace(updatedControl.SettingsJson) ? "{}" : updatedControl.SettingsJson.Trim();
        control.DisplayName = string.IsNullOrWhiteSpace(updatedControl.DisplayName) ? control.ControlType : updatedControl.DisplayName.Trim();

        var notesMarkdown = updatedControl.NotesMarkdown?.Trim() ?? string.Empty;
        control.NotesMarkdown = notesMarkdown;
        control.NotesHtml = string.IsNullOrWhiteSpace(notesMarkdown)
            ? string.Empty
            : Markdig.Markdown.ToHtml(notesMarkdown, new MarkdownPipelineBuilder().DisableHtml().Build());

        control.SequenceOrder = updatedControl.SequenceOrder;
        control.IsActive = updatedControl.IsActive;

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
