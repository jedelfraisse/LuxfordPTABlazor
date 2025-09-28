using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Shared.Models;
using LuxfordPTAWeb.Shared.Enums;
using LuxfordPTAWeb.Shared.DTOs; // Added DTO namespace
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

[ApiController]
[Route("api/events/{eventId}/days")]
[Authorize(Roles = "Admin,BoardMember")]
public class EventDayController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<EventDayController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public EventDayController(ApplicationDbContext db, ILogger<EventDayController> logger, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _logger = logger;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDay>>> GetEventDays(int eventId)
    {
        try
        {
            var eventItem = await _db.Events.FindAsync(eventId);
            if (eventItem == null)
            {
                return NotFound("Event not found");
            }

            // Check permissions
            if (!await CanUserAccessEvent(eventItem))
            {
                return Forbid();
            }

            var eventDays = await _db.EventDays
                .Where(ed => ed.EventId == eventId)
                .OrderBy(ed => ed.DayNumber)
                .ToListAsync();

            return Ok(eventDays);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event days for event {EventId}", eventId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("{dayId}")]
    public async Task<ActionResult<EventDay>> GetEventDay(int eventId, int dayId)
    {
        try
        {
            var eventItem = await _db.Events.FindAsync(eventId);
            if (eventItem == null)
            {
                return NotFound("Event not found");
            }

            if (!await CanUserAccessEvent(eventItem))
            {
                return Forbid();
            }

            var eventDay = await _db.EventDays
                .FirstOrDefaultAsync(ed => ed.Id == dayId && ed.EventId == eventId);

            if (eventDay == null)
            {
                return NotFound("Event day not found");
            }

            return Ok(eventDay);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event day {DayId} for event {EventId}", dayId, eventId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<EventDay>> CreateEventDay(int eventId, [FromBody] EventDay eventDay)
    {
        try
        {
            var eventItem = await _db.Events.FindAsync(eventId);
            if (eventItem == null)
            {
                return NotFound("Event not found");
            }

            if (!await CanUserModifyEvent(eventItem))
            {
                return Forbid();
            }

            // No need to set IsMultiDay; multi-day status is inferred from EventDays.Count > 1

            // Set the event ID and validate day number
            eventDay.EventId = eventId;
            
            if (eventDay.DayNumber <= 0)
            {
                // Auto-assign day number if not provided
                var maxDayNumber = await _db.EventDays
                    .Where(ed => ed.EventId == eventId)
                    .MaxAsync(ed => (int?)ed.DayNumber) ?? 0;
                eventDay.DayNumber = maxDayNumber + 1;
            }
            else
            {
                // Check if day number already exists
                var existingDay = await _db.EventDays
                    .AnyAsync(ed => ed.EventId == eventId && ed.DayNumber == eventDay.DayNumber);
                if (existingDay)
                {
                    return BadRequest($"Day number {eventDay.DayNumber} already exists for this event");
                }
            }

            _db.EventDays.Add(eventDay);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventDay), 
                new { eventId = eventId, dayId = eventDay.Id }, 
                eventDay);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event day for event {EventId}", eventId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{dayId}")]
    public async Task<IActionResult> UpdateEventDay(int eventId, int dayId, [FromBody] EventDay updatedEventDay)
    {
        try
        {
            var eventItem = await _db.Events.FindAsync(eventId);
            if (eventItem == null)
            {
                return NotFound("Event not found");
            }

            if (!await CanUserModifyEvent(eventItem))
            {
                return Forbid();
            }

            var eventDay = await _db.EventDays
                .FirstOrDefaultAsync(ed => ed.Id == dayId && ed.EventId == eventId);

            if (eventDay == null)
            {
                return NotFound("Event day not found");
            }

            // Check if day number is being changed and conflicts with existing
            if (updatedEventDay.DayNumber != eventDay.DayNumber)
            {
                var conflictingDay = await _db.EventDays
                    .AnyAsync(ed => ed.EventId == eventId && 
                                  ed.DayNumber == updatedEventDay.DayNumber && 
                                  ed.Id != dayId);
                if (conflictingDay)
                {
                    return BadRequest($"Day number {updatedEventDay.DayNumber} already exists for this event");
                }
            }

            // Update fields
            eventDay.DayNumber = updatedEventDay.DayNumber;
            eventDay.Date = updatedEventDay.Date;
            eventDay.DayTitle = updatedEventDay.DayTitle;
            eventDay.Description = updatedEventDay.Description;
            eventDay.Location = updatedEventDay.Location;
            eventDay.StartTime = updatedEventDay.StartTime;
            eventDay.EndTime = updatedEventDay.EndTime;
            eventDay.IsActive = updatedEventDay.IsActive;
            eventDay.SpecialInstructions = updatedEventDay.SpecialInstructions;
            eventDay.MaxAttendees = updatedEventDay.MaxAttendees;
            eventDay.EstimatedAttendees = updatedEventDay.EstimatedAttendees;
            eventDay.WeatherBackupPlan = updatedEventDay.WeatherBackupPlan;

            await _db.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event day {DayId} for event {EventId}", dayId, eventId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("{dayId}")]
    public async Task<IActionResult> DeleteEventDay(int eventId, int dayId)
    {
        try
        {
            var eventItem = await _db.Events
                .Include(e => e.EventDays)
                .FirstOrDefaultAsync(e => e.Id == eventId);
                
            if (eventItem == null)
            {
                return NotFound("Event not found");
            }

            if (!await CanUserModifyEvent(eventItem))
            {
                return Forbid();
            }

            var eventDay = await _db.EventDays
                .FirstOrDefaultAsync(ed => ed.Id == dayId && ed.EventId == eventId);

            if (eventDay == null)
            {
                return NotFound("Event day not found");
            }

            _db.EventDays.Remove(eventDay);

            // No need to set IsMultiDay; multi-day status is inferred from EventDays.Count > 1

            await _db.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event day {DayId} for event {EventId}", dayId, eventId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{dayId}/copy")]
    public async Task<ActionResult<EventDay>> CopyEventDay(int eventId, int dayId, [FromBody] CopyEventDayRequestDTO request)
    {
        try
        {
            var sourceEvent = await _db.Events.FindAsync(eventId);
            var targetEvent = await _db.Events.FindAsync(request.TargetEventId);

            if (sourceEvent == null || targetEvent == null)
            {
                return NotFound("Source or target event not found");
            }

            if (!await CanUserAccessEvent(sourceEvent) || !await CanUserModifyEvent(targetEvent))
            {
                return Forbid();
            }

            var sourceDay = await _db.EventDays
                .FirstOrDefaultAsync(ed => ed.Id == dayId && ed.EventId == eventId);

            if (sourceDay == null)
            {
                return NotFound("Source event day not found");
            }

            // Create new event day based on source
            var newEventDay = new EventDay
            {
                EventId = request.TargetEventId,
                DayNumber = request.NewDayNumber > 0 ? request.NewDayNumber : await GetNextDayNumber(request.TargetEventId),
                Date = request.NewDate ?? sourceDay.Date.AddYears(1), // Default to same date next year
                DayTitle = sourceDay.DayTitle,
                Description = sourceDay.Description,
                Location = sourceDay.Location,
                StartTime = sourceDay.StartTime,
                EndTime = sourceDay.EndTime,
                IsActive = sourceDay.IsActive,
                SpecialInstructions = sourceDay.SpecialInstructions,
                MaxAttendees = sourceDay.MaxAttendees,
                EstimatedAttendees = sourceDay.EstimatedAttendees,
                WeatherBackupPlan = sourceDay.WeatherBackupPlan
            };

            _db.EventDays.Add(newEventDay);
            
            // No need to set IsMultiDay; multi-day status is inferred from EventDays.Count > 1

            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventDay), 
                new { eventId = request.TargetEventId, dayId = newEventDay.Id }, 
                newEventDay);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying event day {DayId} from event {EventId} to event {TargetEventId}", 
                dayId, eventId, request.TargetEventId);
            return BadRequest(new { Error = ex.Message });
        }
    }

    private async Task<bool> CanUserAccessEvent(Event eventItem)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return false;

        var userRoles = await _userManager.GetRolesAsync(currentUser);
        return userRoles.Contains("Admin") || 
               userRoles.Contains("BoardMember") || 
               eventItem.EventCoordinatorId == currentUser.Id;
    }

    private async Task<bool> CanUserModifyEvent(Event eventItem)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return false;

        var userRoles = await _userManager.GetRolesAsync(currentUser);
        var isAuthorizedUser = userRoles.Contains("Admin") || 
                              userRoles.Contains("BoardMember") || 
                              eventItem.EventCoordinatorId == currentUser.Id;

        // Additional check: coordinators can only modify events in Planning or SubmittedForApproval status
        if (!userRoles.Contains("Admin") && !userRoles.Contains("BoardMember"))
        {
            return isAuthorizedUser && 
                   (eventItem.Status == EventStatus.Planning || eventItem.Status == EventStatus.SubmittedForApproval);
        }

        return isAuthorizedUser;
    }

    private async Task<int> GetNextDayNumber(int eventId)
    {
        var maxDayNumber = await _db.EventDays
            .Where(ed => ed.EventId == eventId)
            .MaxAsync(ed => (int?)ed.DayNumber) ?? 0;
        return maxDayNumber + 1;
    }
}