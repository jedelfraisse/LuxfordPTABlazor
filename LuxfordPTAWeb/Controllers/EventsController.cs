using LuxfordPTAWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
	private readonly ApplicationDbContext _db;
	public EventsController(ApplicationDbContext db) => _db = db;

	[HttpGet]
	public async Task<IEnumerable<Event>> Get() => await _db.Events.ToListAsync();


	[HttpGet("upcoming")]
	public async Task<IEnumerable<Event>> GetUpcoming()
	{
		var now = DateTime.UtcNow.Date;
		var in30Days = now.AddDays(30);

		// Get events in the next 30 days, ordered by date, take up to 5
		return await _db.Events
			.Where(e => e.Date >= now && e.Date <= in30Days)
			.OrderBy(e => e.Date)
			.Take(5)
			.ToListAsync();
	}
}
