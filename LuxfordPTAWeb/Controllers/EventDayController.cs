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
    public async Task<ActionResult<EventDay>> CreateEventDay(int eventId, [FromBody] CreateEventDayDTO createEventDayDto)
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

            // Validate that the DTO EventId matches the route parameter
            if (createEventDayDto.EventId != eventId)
            {
                return BadRequest("EventId in request body must match the route parameter");
            }

            // Check if day number already exists
            if (createEventDayDto.DayNumber > 0)
            {
                var existingDay = await _db.EventDays
                    .AnyAsync(ed => ed.EventId == eventId && ed.DayNumber == createEventDayDto.DayNumber);
                if (existingDay)
                {
                    return BadRequest($"Day number {createEventDayDto.DayNumber} already exists for this event");
                }
            }
            else
            {
                // Auto-assign day number if not provided or invalid
                var maxDayNumber = await _db.EventDays
                    .Where(ed => ed.EventId == eventId)
                    .MaxAsync(ed => (int?)ed.DayNumber) ?? 0;
                createEventDayDto.DayNumber = maxDayNumber + 1;
            }

            // Create EventDay from DTO
            var eventDay = new EventDay
            {
                EventId = createEventDayDto.EventId,
                DayNumber = createEventDayDto.DayNumber,
                Date = createEventDayDto.Date,
                DayTitle = createEventDayDto.DayTitle,
                Description = createEventDayDto.Description,
                Location = createEventDayDto.Location,
                StartTime = createEventDayDto.StartTime,
                EndTime = createEventDayDto.EndTime,
                IsActive = createEventDayDto.IsActive,
                SpecialInstructions = createEventDayDto.SpecialInstructions,
                MaxAttendees = createEventDayDto.MaxAttendees,
                EstimatedAttendees = createEventDayDto.EstimatedAttendees,
                WeatherBackupPlan = createEventDayDto.WeatherBackupPlan
            };

            _db.EventDays.Add(eventDay);
            await _db.SaveChangesAsync();

            // Load the Event navigation property for the response
            await _db.Entry(eventDay).Reference(ed => ed.Event).LoadAsync();

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
    public async Task<IActionResult> UpdateEventDay(int eventId, int dayId, [FromBody] UpdateEventDayDTO updateEventDayDto)
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
            if (updateEventDayDto.DayNumber != eventDay.DayNumber)
            {
                var conflictingDay = await _db.EventDays
                    .AnyAsync(ed => ed.EventId == eventId && 
                                  ed.DayNumber == updateEventDayDto.DayNumber && 
                                  ed.Id != dayId);
                if (conflictingDay)
                {
                    return BadRequest($"Day number {updateEventDayDto.DayNumber} already exists for this event");
                }
            }

            // Update fields from DTO
            eventDay.DayNumber = updateEventDayDto.DayNumber;
            eventDay.Date = updateEventDayDto.Date;
            eventDay.DayTitle = updateEventDayDto.DayTitle;
            eventDay.Description = updateEventDayDto.Description;
            eventDay.Location = updateEventDayDto.Location;
            eventDay.StartTime = updateEventDayDto.StartTime;
            eventDay.EndTime = updateEventDayDto.EndTime;
            eventDay.IsActive = updateEventDayDto.IsActive;
            eventDay.SpecialInstructions = updateEventDayDto.SpecialInstructions;
            eventDay.MaxAttendees = updateEventDayDto.MaxAttendees;
            eventDay.EstimatedAttendees = updateEventDayDto.EstimatedAttendees;
            eventDay.WeatherBackupPlan = updateEventDayDto.WeatherBackupPlan;

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