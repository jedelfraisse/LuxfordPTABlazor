using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Shared.Models;
using LuxfordPTAWeb.Shared.Enums;
using LuxfordPTAWeb.Shared.DTOs; // Added DTO namespace
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
                .Include(e => e.EventCatSub)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .Include(e => e.EventDays.OrderBy(d => d.DayNumber))
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
                .Include(e => e.EventCatSub)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .Include(e => e.EventDays.OrderBy(d => d.DayNumber))
                .Include(e => e.SourceEvent)
                .Include(e => e.CopiedEvents)
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

    [HttpGet("by-slug/{slug}")]
    public async Task<ActionResult<Event>> GetBySlug(string slug, [FromQuery] string? instance = null, [FromQuery] bool showAll = false)
    {
        try
        {
            var query = _db.Events
                .Include(e => e.EventCat)
                .Include(e => e.EventCatSub)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .Include(e => e.EventDays.OrderBy(d => d.DayNumber))
                .Include(e => e.SourceEvent)
                .Include(e => e.CopiedEvents)
                .Where(e => e.Slug == slug);

            if (!string.IsNullOrEmpty(instance))
            {
                // Filter by specific school year or instance
                query = query.Where(e => e.SchoolYear.Name.Contains(instance));
            }

            var events = await query.ToListAsync();

            if (!events.Any())
            {
                return NotFound($"No events found with slug '{slug}'");
            }

            Event selectedEvent;

            if (showAll)
            {
                // Return the most recent event but include all related instances
                selectedEvent = events
                    .Where(e => e.Status == EventStatus.Active || e.Status == EventStatus.InProgress)
                    .OrderByDescending(e => e.Date)
                    .FirstOrDefault() ?? events.OrderByDescending(e => e.Date).First();
            }
            else
            {
                // Smart resolution: Active/InProgress → Most Recent Completed → Planning
                selectedEvent = events.FirstOrDefault(e => e.Status == EventStatus.Active || e.Status == EventStatus.InProgress)
                              ?? events.Where(e => e.Status == EventStatus.Completed).OrderByDescending(e => e.Date).FirstOrDefault()
                              ?? events.Where(e => e.Status == EventStatus.Planning).OrderByDescending(e => e.Date).FirstOrDefault()
                              ?? events.First();
            }

            // Check permissions for non-active events
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = currentUser != null ? await _userManager.GetRolesAsync(currentUser) : new List<string>();
            var isAuthorizedUser = userRoles.Contains("Admin") || userRoles.Contains("BoardMember") ||
                                   (currentUser != null && selectedEvent.EventCoordinatorId == currentUser.Id);

            if (selectedEvent.Status != EventStatus.Active && !isAuthorizedUser)
            {
                return Forbid();
            }

            return Ok(selectedEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event by slug {Slug}", slug);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("dashboard-summary")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<ActionResult<EventDashboardSummaryDTO>> GetDashboardSummary()
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
    public async Task<ActionResult<EventDashboardSummaryDTO>> GetDashboardSummary(int schoolYearId)
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

            var summary = new EventDashboardSummaryDTO
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

            // Generate slug if not provided
            if (string.IsNullOrEmpty(eventItem.Slug))
            {
                eventItem.Slug = Event.GenerateSlug(eventItem.Title);
            }

            // Ensure slug is unique within the school year
            var existingSlug = await _db.Events
                .AnyAsync(e => e.Slug == eventItem.Slug && e.SchoolYearId == eventItem.SchoolYearId);
            if (existingSlug)
            {
                // Append year to make it unique
                var schoolYear = await _db.SchoolYears.FindAsync(eventItem.SchoolYearId);
                eventItem.Slug += $"-{schoolYear?.Name?.Replace(" ", "").ToLower()}";
            }

            _db.Events.Add(eventItem);
            await _db.SaveChangesAsync();

            // Load navigation properties for return
            await _db.Entry(eventItem).Reference(e => e.EventCat).LoadAsync();
            await _db.Entry(eventItem).Reference(e => e.EventCatSub).LoadAsync();
            await _db.Entry(eventItem).Reference(e => e.SchoolYear).LoadAsync();
            await _db.Entry(eventItem).Reference(e => e.EventCoordinator).LoadAsync();
            await _db.Entry(eventItem).Collection(e => e.EventDays).LoadAsync();

            return CreatedAtAction(nameof(Get), new { id = eventItem.Id }, eventItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{id}/copy")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<ActionResult<Event>> CopyEvent(int id, [FromBody] CopyEventRequestDTO request)
    {
        try
        {
            var sourceEvent = await _db.Events
                .Include(e => e.EventDays)
                .Include(e => e.EventCat)
                .Include(e => e.EventCatSub)
                .Include(e => e.SchoolYear)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (sourceEvent == null)
            {
                return NotFound("Source event not found");
            }

            var targetSchoolYear = await _db.SchoolYears.FindAsync(request.TargetSchoolYearId);
            if (targetSchoolYear == null)
            {
                return BadRequest("Target school year not found");
            }

            // Create new event based on source
            var newEvent = new Event
            {
                Title = request.NewTitle ?? sourceEvent.Title,
                Date = request.NewStartDate ?? sourceEvent.Date.AddYears(1),
                Description = sourceEvent.Description,
                Location = sourceEvent.Location,
                ImageUrl = sourceEvent.ImageUrl,
                Link = sourceEvent.Link,
                EventCoordinatorId = request.NewCoordinatorId ?? sourceEvent.EventCoordinatorId,
                Status = EventStatus.Planning, // New events start in planning
                EventStartTime = UpdateDateKeepTime(request.NewStartDate ?? sourceEvent.Date.AddYears(1), sourceEvent.EventStartTime),
                EventEndTime = UpdateDateKeepTime(request.NewStartDate ?? sourceEvent.Date.AddYears(1), sourceEvent.EventEndTime),
                SetupStartTime = sourceEvent.SetupStartTime.HasValue 
                    ? UpdateDateKeepTime(request.NewStartDate ?? sourceEvent.Date.AddYears(1), sourceEvent.SetupStartTime.Value)
                    : null,
                CleanupEndTime = sourceEvent.CleanupEndTime.HasValue 
                    ? UpdateDateKeepTime(request.NewStartDate ?? sourceEvent.Date.AddYears(1), sourceEvent.CleanupEndTime.Value)
                    : null,
                MaxAttendees = sourceEvent.MaxAttendees,
                EstimatedAttendees = sourceEvent.EstimatedAttendees,
                RequiresVolunteers = sourceEvent.RequiresVolunteers,
                RequiresSetup = sourceEvent.RequiresSetup,
                RequiresCleanup = sourceEvent.RequiresCleanup,
                Notes = sourceEvent.Notes,
                PublicInstructions = sourceEvent.PublicInstructions,
                WeatherBackupPlan = sourceEvent.WeatherBackupPlan,
                SchoolYearId = request.TargetSchoolYearId,
                EventCatId = sourceEvent.EventCatId,
                EventSubTypeId = sourceEvent.EventSubTypeId,
                Slug = Event.GenerateSlug(request.NewTitle ?? sourceEvent.Title),
                SourceEventId = sourceEvent.Id,
                CopyGeneration = sourceEvent.CopyGeneration + 1
            };

            // Ensure slug is unique
            var baseSlug = newEvent.Slug;
            var counter = 1;
            while (await _db.Events.AnyAsync(e => e.Slug == newEvent.Slug && e.SchoolYearId == request.TargetSchoolYearId))
            {
                newEvent.Slug = $"{baseSlug}-{counter}";
                counter++;
            }

            _db.Events.Add(newEvent);
            await _db.SaveChangesAsync();

            // Copy event days if requested and source event has days
            if (sourceEvent.EventDays.Any() && request.CopyEventDays)
            {
                var dayOffset = request.NewStartDate?.Subtract(sourceEvent.Date).Days ?? 365; // Default 1 year offset

                foreach (var sourceDay in sourceEvent.EventDays.OrderBy(d => d.DayNumber))
                {
                    var newDay = new EventDay
                    {
                        EventId = newEvent.Id,
                        DayNumber = sourceDay.DayNumber,
                        Date = sourceDay.Date.AddDays(dayOffset),
                        DayTitle = sourceDay.DayTitle,
                        Description = sourceDay.Description,
                        Location = sourceDay.Location,
                        StartTime = sourceDay.StartTime?.AddDays(dayOffset),
                        EndTime = sourceDay.EndTime?.AddDays(dayOffset),
                        IsActive = sourceDay.IsActive,
                        SpecialInstructions = sourceDay.SpecialInstructions,
                        MaxAttendees = sourceDay.MaxAttendees,
                        EstimatedAttendees = sourceDay.EstimatedAttendees,
                        WeatherBackupPlan = sourceDay.WeatherBackupPlan
                    };

                    _db.EventDays.Add(newDay);
                }

                await _db.SaveChangesAsync();
            }

            // Load navigation properties for return
            await _db.Entry(newEvent).Reference(e => e.EventCat).LoadAsync();
            await _db.Entry(newEvent).Reference(e => e.EventCatSub).LoadAsync();
            await _db.Entry(newEvent).Reference(e => e.SchoolYear).LoadAsync();
            await _db.Entry(newEvent).Reference(e => e.EventCoordinator).LoadAsync();
            await _db.Entry(newEvent).Collection(e => e.EventDays).LoadAsync();

            return CreatedAtAction(nameof(Get), new { id = newEvent.Id }, newEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying event {EventId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("available-for-copy")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<ActionResult<IEnumerable<Event>>> GetAvailableForCopy([FromQuery] int? excludeSchoolYearId = null)
    {
        try
        {
            var query = _db.Events
                .Include(e => e.EventCat)
                .Include(e => e.EventCatSub)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .Where(e => e.Status == EventStatus.Completed || e.Status == EventStatus.Active)
                .AsQueryable();

            if (excludeSchoolYearId.HasValue)
            {
                query = query.Where(e => e.SchoolYearId != excludeSchoolYearId.Value);
            }

            var events = await query
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting events available for copying");
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
            eventItem.EventSubTypeId = updatedEvent.EventSubTypeId;
            eventItem.ExcelImportId = updatedEvent.ExcelImportId;
            
            // Update slug if title changed
            if (eventItem.Title != updatedEvent.Title && !string.IsNullOrEmpty(updatedEvent.Title))
            {
                var newSlug = Event.GenerateSlug(updatedEvent.Title);
                if (newSlug != eventItem.Slug)
                {
                    // Check if new slug is available
                    var existingSlug = await _db.Events
                        .AnyAsync(e => e.Slug == newSlug && e.SchoolYearId == eventItem.SchoolYearId && e.Id != id);
                    if (!existingSlug)
                    {
                        eventItem.Slug = newSlug;
                    }
                }
            }

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
                .Include(e => e.EventCatSub)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .Include(e => e.EventDays.OrderBy(d => d.DayNumber))
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
                .Include(e => e.EventCatSub)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .Include(e => e.EventDays.OrderBy(d => d.DayNumber))
                .Where(e => e.EventCatId == eventCatId && e.Status == EventStatus.Active)
                .OrderBy(e => e.Date)
                .ToListAsync();

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting events by category");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("by-category/{slug}")]
    public async Task<ActionResult<IEnumerable<Event>>> GetByEventCatSlug(string slug)
    {
        try
        {
            var events = await _db.Events
                .Include(e => e.EventCat)
                .Include(e => e.EventCatSub)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .Include(e => e.EventDays.OrderBy(d => d.DayNumber))
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
            var eventItem = await _db.Events
                .Include(e => e.EventDays)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (eventItem == null)
            {
                return NotFound();
            }

            // Remove all event days first
            if (eventItem.EventDays.Any())
            {
                _db.EventDays.RemoveRange(eventItem.EventDays);
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
                .Include(e => e.EventCatSub)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .Include(e => e.EventDays)
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

            var currentUser = await _userManager.GetUserAsync(User);
            
            eventItem.Status = EventStatus.Active;
            eventItem.ApprovedByUserId = currentUser?.Id;
            eventItem.ApprovedDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving event {EventId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("by-school-year/{schoolYearId}")]
    public async Task<ActionResult<IEnumerable<Event>>> GetBySchoolYear(int schoolYearId)
    {
        try
        {
            var events = await _db.Events
                .Include(e => e.EventCat)
                .Include(e => e.EventCatSub)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .Include(e => e.EventDays.OrderBy(d => d.DayNumber))
                .Where(e => e.SchoolYearId == schoolYearId && e.Status == EventStatus.Active)
                .OrderBy(e => e.Date)
                .ToListAsync();

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting events for school year {SchoolYearId}", schoolYearId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    private DateTime UpdateDateKeepTime(DateTime newDate, DateTime originalDateTime)
    {
        return newDate.Date.Add(originalDateTime.TimeOfDay);
    }
}