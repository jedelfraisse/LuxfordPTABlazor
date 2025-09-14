using LuxfordPTAWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public EventsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<Event>> Get()
    {
        return await _db.Events
            .Include(e => e.EventType)
            .Include(e => e.SchoolYear)
            .ToListAsync();
    }

    [HttpGet("upcoming")]
    public async Task<IEnumerable<Event>> GetUpcoming()
    {
        var now = DateTime.UtcNow.Date;
        var in30Days = now.AddDays(30);

        return await _db.Events
            .Include(e => e.EventType)
            .Include(e => e.SchoolYear)
            .Where(e => e.Date >= now && e.Date <= in30Days)
            .OrderBy(e => e.Date)
            .Take(5)
            .ToListAsync();
    }

    [HttpGet("by-type/{eventTypeId}")]
    public async Task<IEnumerable<Event>> GetByEventType(int eventTypeId)
    {
        return await _db.Events
            .Include(e => e.EventType)
            .Include(e => e.SchoolYear)
            .Where(e => e.EventTypeId == eventTypeId)
            .OrderBy(e => e.Date)
            .ToListAsync();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Delete(int id)
    {
        var eventItem = await _db.Events.FindAsync(id);
        if (eventItem == null)
        {
            return NotFound();
        }

        _db.Events.Remove(eventItem);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
