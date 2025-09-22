using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Shared.Models;
using LuxfordPTAWeb.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<EventsController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public EventsController(ApplicationDbContext db, ILogger<EventsController> logger, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _logger = logger;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Event>>> Get()
    {
        try
        {
            var events = await _db.Events
                .Include(e => e.EventCat)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .Where(e => e.Status == EventStatus.Active)
                .ToListAsync();

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all events");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Event>> Get(int id)
    {
        try
        {
            var eventItem = await _db.Events
                .Include(e => e.EventCat)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = currentUser != null ? await _userManager.GetRolesAsync(currentUser) : new List<string>();
            var isAuthorizedUser = userRoles.Contains("Admin") || userRoles.Contains("BoardMember") ||
                                   (currentUser != null && eventItem.EventCoordinatorId == currentUser.Id);

            if (eventItem.Status != EventStatus.Active && !isAuthorizedUser)
            {
                return Forbid();
            }

            return Ok(eventItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event {EventId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("dashboard-summary")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<ActionResult<EventDashboardSummary>> GetDashboardSummary()
    {
        try
        {
            // Get current school year when no specific year is provided
            var currentSchoolYear = await _db.SchoolYears
                .FirstOrDefaultAsync(sy => sy.Status == LuxfordPTAWeb.Shared.Enums.SchoolYearStatus.CurrentYear);

            if (currentSchoolYear == null)
            {
                return BadRequest("No current school year found");
            }

            return await GetDashboardSummary(currentSchoolYear.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard summary for current school year");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("dashboard-summary/{schoolYearId}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<ActionResult<EventDashboardSummary>> GetDashboardSummary(int schoolYearId)
    {
        try
        {
            // Filter events by the provided school year ID
            var allEvents = await _db.Events
                .Include(e => e.SchoolYear)
                .Where(e => e.SchoolYearId == schoolYearId)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var next30Days = now.AddDays(30);
            var next7Days = now.AddDays(7);

            var summary = new EventDashboardSummary
            {
                TotalEvents = allEvents.Count,
                PendingApproval = allEvents.Count(e => e.Status == EventStatus.SubmittedForApproval),
                ActiveEvents = allEvents.Count(e => e.Status == EventStatus.Active),
                InProgressEvents = allEvents.Count(e => e.Status == EventStatus.InProgress),
                CompletedEvents = allEvents.Count(e => e.Status == EventStatus.Completed),
                CancelledEvents = allEvents.Count(e => e.Status == EventStatus.Cancelled),
                PlanningEvents = allEvents.Count(e => e.Status == EventStatus.Planning),
                UpcomingNext7Days = allEvents.Count(e => e.Date >= now && e.Date <= next7Days && 
                    (e.Status == EventStatus.Active || e.Status == EventStatus.InProgress)),
                UpcomingNext30Days = allEvents.Count(e => e.Date >= now && e.Date <= next30Days && 
                    (e.Status == EventStatus.Active || e.Status == EventStatus.InProgress)),
                NeedsAttention = allEvents.Count(e => 
                    e.Status == EventStatus.SubmittedForApproval ||
                    (e.Status == EventStatus.InProgress && e.Date < now.AddDays(-1)) ||
                    (e.RequiresVolunteers && string.IsNullOrEmpty(e.ExcelImportId) && 
                     e.Date >= now && e.Date <= next30Days)),
                RequiringVolunteers = allEvents.Count(e => e.RequiresVolunteers && 
                    e.Date >= now && (e.Status == EventStatus.Active || e.Status == EventStatus.InProgress))
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard summary for school year {SchoolYearId}", schoolYearId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<ActionResult<Event>> Post([FromBody] Event eventItem)
    {
        try
        {
            // Validate coordinator assignment
            if (!string.IsNullOrEmpty(eventItem.EventCoordinatorId))
            {
                var coordinator = await _userManager.FindByIdAsync(eventItem.EventCoordinatorId);
                if (coordinator == null)
                {
                    return BadRequest("Invalid event coordinator ID");
                }
            }

            _db.Events.Add(eventItem);
            await _db.SaveChangesAsync();

            // Load navigation properties for return
            await _db.Entry(eventItem).Reference(e => e.EventCat).LoadAsync();
            await _db.Entry(eventItem).Reference(e => e.SchoolYear).LoadAsync();
            await _db.Entry(eventItem).Reference(e => e.EventCoordinator).LoadAsync();

            return CreatedAtAction(nameof(Get), new { id = eventItem.Id }, eventItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Put(int id, [FromBody] Event updatedEvent)
    {
        try
        {
            var eventItem = await _db.Events
                .Include(e => e.EventCoordinator)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = currentUser != null ? await _userManager.GetRolesAsync(currentUser) : new List<string>();
            var isCoordinator = currentUser != null && eventItem.EventCoordinatorId == currentUser.Id;
            var isAuthorizedUser = userRoles.Contains("Admin") || userRoles.Contains("BoardMember") || isCoordinator;

            if (!isAuthorizedUser)
            {
                return Forbid();
            }

            // Validate coordinator assignment (only admins/board members can change coordinator)
            if (!string.IsNullOrEmpty(updatedEvent.EventCoordinatorId) && updatedEvent.EventCoordinatorId != eventItem.EventCoordinatorId)
            {
                if (!userRoles.Contains("Admin") && !userRoles.Contains("BoardMember"))
                {
                    return Forbid("Only admins and board members can reassign event coordinators");
                }

                var coordinator = await _userManager.FindByIdAsync(updatedEvent.EventCoordinatorId);
                if (coordinator == null)
                {
                    return BadRequest("Invalid event coordinator ID");
                }
            }

            // Update fields
            eventItem.Title = updatedEvent.Title;
            eventItem.Date = updatedEvent.Date;
            eventItem.Description = updatedEvent.Description;
            eventItem.Location = updatedEvent.Location;
            eventItem.ImageUrl = updatedEvent.ImageUrl;
            eventItem.Link = updatedEvent.Link;
            eventItem.Status = updatedEvent.Status;
            eventItem.EventStartTime = updatedEvent.EventStartTime;
            eventItem.EventEndTime = updatedEvent.EventEndTime;
            eventItem.SetupStartTime = updatedEvent.SetupStartTime;
            eventItem.CleanupEndTime = updatedEvent.CleanupEndTime;
            eventItem.MaxAttendees = updatedEvent.MaxAttendees;
            eventItem.EstimatedAttendees = updatedEvent.EstimatedAttendees;
            eventItem.RequiresVolunteers = updatedEvent.RequiresVolunteers;
            eventItem.RequiresSetup = updatedEvent.RequiresSetup;
            eventItem.RequiresCleanup = updatedEvent.RequiresCleanup;
            eventItem.Notes = updatedEvent.Notes;
            eventItem.PublicInstructions = updatedEvent.PublicInstructions;
            eventItem.WeatherBackupPlan = updatedEvent.WeatherBackupPlan;
            eventItem.SchoolYearId = updatedEvent.SchoolYearId;
            eventItem.EventCatId = updatedEvent.EventCatId;
            eventItem.ExcelImportId = updatedEvent.ExcelImportId;

            // Only admins/board members can change coordinator
            if (userRoles.Contains("Admin") || userRoles.Contains("BoardMember"))
            {
                eventItem.EventCoordinatorId = updatedEvent.EventCoordinatorId;
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event {EventId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<IEnumerable<Event>>> GetUpcoming()
    {
        try
        {
            var now = DateTime.UtcNow.Date;
            var in30Days = now.AddDays(30);

            var events = await _db.Events
                .Include(e => e.EventCat)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .Where(e => e.Date >= now && e.Date <= in30Days && e.Status == EventStatus.Active)
                .OrderBy(e => e.Date)
                .Take(5)
                .ToListAsync();

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming events");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("by-type/{eventcatid}")]
    public async Task<ActionResult<IEnumerable<Event>>> GetByEventCat(int eventCatId)
    {
        try
        {
            var events = await _db.Events
                .Include(e => e.EventCat)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .Where(e => e.EventCatId == eventCatId && e.Status == EventStatus.Active)
                .OrderBy(e => e.Date)
                .ToListAsync();

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting events by type");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("by-category/{slug}")]
    public async Task<ActionResult<IEnumerable<Event>>> GetByEventTypeSlug(string slug)
    {
        try
        {
            var events = await _db.Events
                .Include(e => e.EventCat)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .Where(e => e.EventCat.Slug == slug && e.EventCat.IsActive && e.Status == EventStatus.Active)
                .OrderBy(e => e.Date)
                .ToListAsync();

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting events by category slug: {Slug}", slug);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{id}/coordinator")]
    [Authorize]
    public async Task<ActionResult<ApplicationUser>> GetCoordinator(int id)
    {
        try
        {
            var eventItem = await _db.Events
                .Include(e => e.EventCoordinator)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            if (eventItem.EventCoordinator == null)
            {
                return Ok(null);
            }

            return Ok(eventItem.EventCoordinator);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event coordinator for event {EventId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Delete(int id)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("all-admin")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<ActionResult<IEnumerable<Event>>> GetAllAdmin([FromQuery] int? schoolYearId = null)
    {
        try
        {
            var query = _db.Events
                .Include(e => e.EventCat)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .AsQueryable();

            // Filter by school year if provided
            if (schoolYearId.HasValue && schoolYearId.Value > 0)
            {
                query = query.Where(e => e.SchoolYearId == schoolYearId.Value);
            }

            var events = await query.ToListAsync();
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all events for admin");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> ApproveEvent(int id)
    {
        try
        {
            var eventItem = await _db.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            if (eventItem.Status != EventStatus.SubmittedForApproval)
            {
                return BadRequest("Event is not in SubmittedForApproval status");
            }

            eventItem.Status = EventStatus.Active;
            await _db.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving event {EventId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }
}