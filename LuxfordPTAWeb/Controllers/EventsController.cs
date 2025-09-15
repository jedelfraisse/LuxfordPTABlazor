using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Data.Enums;
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
    public async Task<ActionResult<IEnumerable<EventDto>>> Get()
    {
        try
        {
            var events = await _db.Events
                .Include(e => e.EventType)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .Where(e => e.Status == EventStatus.Active) // Only show active events to public
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Date = e.Date,
                    Description = e.Description,
                    Location = e.Location,
                    ImageUrl = e.ImageUrl,
                    Link = e.Link,
                    Status = e.Status,
                    EventStartTime = e.EventStartTime,
                    EventEndTime = e.EventEndTime,
                    SetupStartTime = e.SetupStartTime,
                    CleanupEndTime = e.CleanupEndTime,
                    MaxAttendees = e.MaxAttendees,
                    EstimatedAttendees = e.EstimatedAttendees,
                    RequiresVolunteers = e.RequiresVolunteers,
                    RequiresSetup = e.RequiresSetup,
                    RequiresCleanup = e.RequiresCleanup,
                    PublicInstructions = e.PublicInstructions,
                    WeatherBackupPlan = e.WeatherBackupPlan,
                    EventCoordinatorName = e.EventCoordinator != null ? e.EventCoordinator.Email : null,
                    EventType = e.EventType != null ? new EventTypeDto
                    {
                        Id = e.EventType.Id,
                        Name = e.EventType.Name,
                        Slug = e.EventType.Slug,
                        Description = e.EventType.Description,
                        DisplayOrder = e.EventType.DisplayOrder,
                        IsActive = e.EventType.IsActive,
                        Size = (int)e.EventType.Size,
                        Icon = e.EventType.Icon,
                        ColorClass = e.EventType.ColorClass
                    } : null,
                    SchoolYear = e.SchoolYear != null ? new SchoolYearDto
                    {
                        Id = e.SchoolYear.Id,
                        Name = e.SchoolYear.Name,
                        StartDate = e.SchoolYear.StartDate,
                        EndDate = e.SchoolYear.EndDate
                    } : null
                })
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
    public async Task<ActionResult<EventDto>> Get(int id)
    {
        try
        {
            var eventItem = await _db.Events
                .Include(e => e.EventType)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            // Check permissions - only coordinators, board members, and admins can see non-active events
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = currentUser != null ? await _userManager.GetRolesAsync(currentUser) : new List<string>();
            var isAuthorizedUser = userRoles.Contains("Admin") || userRoles.Contains("BoardMember") || 
                                  (currentUser != null && eventItem.EventCoordinatorId == currentUser.Id);

            if (eventItem.Status != EventStatus.Active && !isAuthorizedUser)
            {
                return Forbid();
            }

            var eventDto = new EventDto
            {
                Id = eventItem.Id,
                Title = eventItem.Title,
                Date = eventItem.Date,
                Description = eventItem.Description,
                Location = eventItem.Location,
                ImageUrl = eventItem.ImageUrl,
                Link = eventItem.Link,
                Status = eventItem.Status,
                EventStartTime = eventItem.EventStartTime,
                EventEndTime = eventItem.EventEndTime,
                SetupStartTime = eventItem.SetupStartTime,
                CleanupEndTime = eventItem.CleanupEndTime,
                MaxAttendees = eventItem.MaxAttendees,
                EstimatedAttendees = eventItem.EstimatedAttendees,
                RequiresVolunteers = eventItem.RequiresVolunteers,
                RequiresSetup = eventItem.RequiresSetup,
                RequiresCleanup = eventItem.RequiresCleanup,
                PublicInstructions = eventItem.PublicInstructions,
                WeatherBackupPlan = eventItem.WeatherBackupPlan,
                EventCoordinatorName = eventItem.EventCoordinator?.Email,
                EventCoordinatorId = eventItem.EventCoordinatorId,
                ExcelImportId = eventItem.ExcelImportId,
                // Include internal notes only for authorized users
                Notes = isAuthorizedUser ? eventItem.Notes : string.Empty,
                EventType = eventItem.EventType != null ? new EventTypeDto
                {
                    Id = eventItem.EventType.Id,
                    Name = eventItem.EventType.Name,
                    Slug = eventItem.EventType.Slug,
                    Description = eventItem.EventType.Description,
                    DisplayOrder = eventItem.EventType.DisplayOrder,
                    IsActive = eventItem.EventType.IsActive,
                    Size = (int)eventItem.EventType.Size,
                    Icon = eventItem.EventType.Icon,
                    ColorClass = eventItem.EventType.ColorClass
                } : null,
                SchoolYear = eventItem.SchoolYear != null ? new SchoolYearDto
                {
                    Id = eventItem.SchoolYear.Id,
                    Name = eventItem.SchoolYear.Name,
                    StartDate = eventItem.SchoolYear.StartDate,
                    EndDate = eventItem.SchoolYear.EndDate
                } : null
            };

            return Ok(eventDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event {EventId}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<ActionResult<EventDto>> Post([FromBody] CreateEventDto dto)
    {
        try
        {
            // Validate coordinator assignment
            ApplicationUser? coordinator = null;
            if (!string.IsNullOrEmpty(dto.EventCoordinatorId))
            {
                coordinator = await _userManager.FindByIdAsync(dto.EventCoordinatorId);
                if (coordinator == null)
                {
                    return BadRequest("Invalid event coordinator ID");
                }
            }

            var eventItem = new Event
            {
                Title = dto.Title,
                Date = dto.Date,
                Description = dto.Description,
                Location = dto.Location,
                ImageUrl = dto.ImageUrl ?? string.Empty,
                Link = dto.Link ?? string.Empty,
                EventCoordinatorId = dto.EventCoordinatorId,
                Status = dto.Status,
                EventStartTime = dto.EventStartTime,
                EventEndTime = dto.EventEndTime,
                SetupStartTime = dto.SetupStartTime,
                CleanupEndTime = dto.CleanupEndTime,
                MaxAttendees = dto.MaxAttendees,
                EstimatedAttendees = dto.EstimatedAttendees,
                RequiresVolunteers = dto.RequiresVolunteers,
                RequiresSetup = dto.RequiresSetup,
                RequiresCleanup = dto.RequiresCleanup,
                Notes = dto.Notes ?? string.Empty,
                PublicInstructions = dto.PublicInstructions ?? string.Empty,
                WeatherBackupPlan = dto.WeatherBackupPlan ?? string.Empty,
                SchoolYearId = dto.SchoolYearId,
                EventTypeId = dto.EventTypeId
            };

            _db.Events.Add(eventItem);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = eventItem.Id }, await MapToDto(eventItem));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Put(int id, [FromBody] CreateEventDto dto)
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

            // Check if current user is the coordinator or has admin rights
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = currentUser != null ? await _userManager.GetRolesAsync(currentUser) : new List<string>();
            var isCoordinator = currentUser != null && eventItem.EventCoordinatorId == currentUser.Id;
            var isAuthorizedUser = userRoles.Contains("Admin") || userRoles.Contains("BoardMember") || isCoordinator;

            if (!isAuthorizedUser)
            {
                return Forbid();
            }

            // Validate coordinator assignment (only admins/board members can change coordinator)
            if (!string.IsNullOrEmpty(dto.EventCoordinatorId) && dto.EventCoordinatorId != eventItem.EventCoordinatorId)
            {
                if (!userRoles.Contains("Admin") && !userRoles.Contains("BoardMember"))
                {
                    return Forbid("Only admins and board members can reassign event coordinators");
                }
                
                var coordinator = await _userManager.FindByIdAsync(dto.EventCoordinatorId);
                if (coordinator == null)
                {
                    return BadRequest("Invalid event coordinator ID");
                }
            }

            // Update fields
            eventItem.Title = dto.Title;
            eventItem.Date = dto.Date;
            eventItem.Description = dto.Description;
            eventItem.Location = dto.Location;
            eventItem.ImageUrl = dto.ImageUrl ?? eventItem.ImageUrl;
            eventItem.Link = dto.Link ?? eventItem.Link;
            eventItem.Status = dto.Status;
            eventItem.EventStartTime = dto.EventStartTime;
            eventItem.EventEndTime = dto.EventEndTime;
            eventItem.SetupStartTime = dto.SetupStartTime;
            eventItem.CleanupEndTime = dto.CleanupEndTime;
            eventItem.MaxAttendees = dto.MaxAttendees;
            eventItem.EstimatedAttendees = dto.EstimatedAttendees;
            eventItem.RequiresVolunteers = dto.RequiresVolunteers;
            eventItem.RequiresSetup = dto.RequiresSetup;
            eventItem.RequiresCleanup = dto.RequiresCleanup;
            eventItem.Notes = dto.Notes ?? eventItem.Notes;
            eventItem.PublicInstructions = dto.PublicInstructions ?? eventItem.PublicInstructions;
            eventItem.WeatherBackupPlan = dto.WeatherBackupPlan ?? eventItem.WeatherBackupPlan;
            eventItem.SchoolYearId = dto.SchoolYearId;
            eventItem.EventTypeId = dto.EventTypeId;

            // Only admins/board members can change coordinator
            if (userRoles.Contains("Admin") || userRoles.Contains("BoardMember"))
            {
                eventItem.EventCoordinatorId = dto.EventCoordinatorId;
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
    public async Task<ActionResult<IEnumerable<EventDto>>> GetUpcoming()
    {
        try
        {
            var now = DateTime.UtcNow.Date;
            var in30Days = now.AddDays(30);

            var events = await _db.Events
                .Include(e => e.EventType)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .Where(e => e.Date >= now && e.Date <= in30Days && e.Status == EventStatus.Active)
                .OrderBy(e => e.Date)
                .Take(5)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Date = e.Date,
                    Description = e.Description,
                    Location = e.Location,
                    ImageUrl = e.ImageUrl,
                    Link = e.Link,
                    Status = e.Status,
                    EventStartTime = e.EventStartTime,
                    EventEndTime = e.EventEndTime,
                    RequiresVolunteers = e.RequiresVolunteers,
                    PublicInstructions = e.PublicInstructions,
                    EventCoordinatorName = e.EventCoordinator != null ? e.EventCoordinator.Email : null,
                    EventType = e.EventType != null ? new EventTypeDto
                    {
                        Id = e.EventType.Id,
                        Name = e.EventType.Name,
                        Slug = e.EventType.Slug,
                        Description = e.EventType.Description,
                        DisplayOrder = e.EventType.DisplayOrder,
                        IsActive = e.EventType.IsActive,
                        Size = (int)e.EventType.Size,
                        Icon = e.EventType.Icon,
                        ColorClass = e.EventType.ColorClass
                    } : null,
                    SchoolYear = e.SchoolYear != null ? new SchoolYearDto
                    {
                        Id = e.SchoolYear.Id,
                        Name = e.SchoolYear.Name,
                        StartDate = e.SchoolYear.StartDate,
                        EndDate = e.SchoolYear.EndDate
                    } : null
                })
                .ToListAsync();

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming events");
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("by-type/{eventTypeId}")]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetByEventType(int eventTypeId)
    {
        try
        {
            var events = await _db.Events
                .Include(e => e.EventType)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .Where(e => e.EventTypeId == eventTypeId && e.Status == EventStatus.Active)
                .OrderBy(e => e.Date)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Date = e.Date,
                    Description = e.Description,
                    Location = e.Location,
                    ImageUrl = e.ImageUrl,
                    Link = e.Link,
                    Status = e.Status,
                    EventStartTime = e.EventStartTime,
                    EventEndTime = e.EventEndTime,
                    RequiresVolunteers = e.RequiresVolunteers,
                    PublicInstructions = e.PublicInstructions,
                    EventCoordinatorName = e.EventCoordinator != null ? e.EventCoordinator.Email : null,
                    EventType = e.EventType != null ? new EventTypeDto
                    {
                        Id = e.EventType.Id,
                        Name = e.EventType.Name,
                        Slug = e.EventType.Slug,
                        Description = e.EventType.Description,
                        DisplayOrder = e.EventType.DisplayOrder,
                        IsActive = e.EventType.IsActive,
                        Size = (int)e.EventType.Size,
                        Icon = e.EventType.Icon,
                        ColorClass = e.EventType.ColorClass
                    } : null,
                    SchoolYear = e.SchoolYear != null ? new SchoolYearDto
                    {
                        Id = e.SchoolYear.Id,
                        Name = e.SchoolYear.Name,
                        StartDate = e.SchoolYear.StartDate,
                        EndDate = e.SchoolYear.EndDate
                    } : null
                })
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
    public async Task<ActionResult<IEnumerable<EventDto>>> GetByEventTypeSlug(string slug)
    {
        try
        {
            var events = await _db.Events
                .Include(e => e.EventType)
                .Include(e => e.SchoolYear)
                .Include(e => e.EventCoordinator)
                .Where(e => e.EventType!.Slug == slug && e.EventType.IsActive && e.Status == EventStatus.Active)
                .OrderBy(e => e.Date)
                .Select(e => new EventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Date = e.Date,
                    Description = e.Description,
                    Location = e.Location,
                    ImageUrl = e.ImageUrl,
                    Link = e.Link,
                    Status = e.Status,
                    EventStartTime = e.EventStartTime,
                    EventEndTime = e.EventEndTime,
                    RequiresVolunteers = e.RequiresVolunteers,
                    PublicInstructions = e.PublicInstructions,
                    EventCoordinatorName = e.EventCoordinator != null ? e.EventCoordinator.Email : null,
                    EventType = e.EventType != null ? new EventTypeDto
                    {
                        Id = e.EventType.Id,
                        Name = e.EventType.Name,
                        Slug = e.EventType.Slug,
                        Description = e.EventType.Description,
                        DisplayOrder = e.EventType.DisplayOrder,
                        IsActive = e.EventType.IsActive,
                        Size = (int)e.EventType.Size,
                        Icon = e.EventType.Icon,
                        ColorClass = e.EventType.ColorClass
                    } : null,
                    SchoolYear = e.SchoolYear != null ? new SchoolYearDto
                    {
                        Id = e.SchoolYear.Id,
                        Name = e.SchoolYear.Name,
                        StartDate = e.SchoolYear.StartDate,
                        EndDate = e.SchoolYear.EndDate
                    } : null
                })
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
    public async Task<ActionResult<EventCoordinatorDto>> GetCoordinator(int id)
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
                return Ok(new EventCoordinatorDto { HasCoordinator = false });
            }

            return Ok(new EventCoordinatorDto
            {
                HasCoordinator = true,
                CoordinatorId = eventItem.EventCoordinatorId!,
                CoordinatorName = eventItem.EventCoordinator.Email ?? "Unknown",
                CoordinatorEmail = eventItem.EventCoordinator.Email ?? "Unknown"
            });
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

    private async Task<EventDto> MapToDto(Event eventItem)
    {
        await _db.Entry(eventItem)
            .Reference(e => e.EventType)
            .LoadAsync();
        await _db.Entry(eventItem)
            .Reference(e => e.SchoolYear)
            .LoadAsync();
        await _db.Entry(eventItem)
            .Reference(e => e.EventCoordinator)
            .LoadAsync();

        return new EventDto
        {
            Id = eventItem.Id,
            Title = eventItem.Title,
            Date = eventItem.Date,
            Description = eventItem.Description,
            Location = eventItem.Location,
            ImageUrl = eventItem.ImageUrl,
            Link = eventItem.Link,
            Status = eventItem.Status,
            EventStartTime = eventItem.EventStartTime,
            EventEndTime = eventItem.EventEndTime,
            SetupStartTime = eventItem.SetupStartTime,
            CleanupEndTime = eventItem.CleanupEndTime,
            MaxAttendees = eventItem.MaxAttendees,
            EstimatedAttendees = eventItem.EstimatedAttendees,
            RequiresVolunteers = eventItem.RequiresVolunteers,
            RequiresSetup = eventItem.RequiresSetup,
            RequiresCleanup = eventItem.RequiresCleanup,
            Notes = eventItem.Notes,
            PublicInstructions = eventItem.PublicInstructions,
            WeatherBackupPlan = eventItem.WeatherBackupPlan,
            EventCoordinatorName = eventItem.EventCoordinator?.Email,
            EventCoordinatorId = eventItem.EventCoordinatorId,
            ExcelImportId = eventItem.ExcelImportId,
            EventType = eventItem.EventType != null ? new EventTypeDto
            {
                Id = eventItem.EventType.Id,
                Name = eventItem.EventType.Name,
                Slug = eventItem.EventType.Slug,
                Description = eventItem.EventType.Description,
                DisplayOrder = eventItem.EventType.DisplayOrder,
                IsActive = eventItem.EventType.IsActive,
                Size = (int)eventItem.EventType.Size,
                Icon = eventItem.EventType.Icon,
                ColorClass = eventItem.EventType.ColorClass
            } : null,
            SchoolYear = eventItem.SchoolYear != null ? new SchoolYearDto
            {
                Id = eventItem.SchoolYear.Id,
                Name = eventItem.SchoolYear.Name,
                StartDate = eventItem.SchoolYear.StartDate,
                EndDate = eventItem.SchoolYear.EndDate
            } : null
        };
    }
}

// Enhanced DTO Classes
public class EventDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    
    // Enhanced fields
    public EventStatus Status { get; set; }
    public DateTime EventStartTime { get; set; }
    public DateTime EventEndTime { get; set; }
    public DateTime? SetupStartTime { get; set; }
    public DateTime? CleanupEndTime { get; set; }
    public int? MaxAttendees { get; set; }
    public int? EstimatedAttendees { get; set; }
    public bool RequiresVolunteers { get; set; }
    public bool RequiresSetup { get; set; }
    public bool RequiresCleanup { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string PublicInstructions { get; set; } = string.Empty;
    public string WeatherBackupPlan { get; set; } = string.Empty;
    public string? EventCoordinatorName { get; set; }
    public string? EventCoordinatorId { get; set; }
    public string? ExcelImportId { get; set; }
    
    public EventTypeDto? EventType { get; set; }
    public SchoolYearDto? SchoolYear { get; set; }
}

public class CreateEventDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Link { get; set; }
    
    // Enhanced fields
    public string? EventCoordinatorId { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Planning;
    public DateTime EventStartTime { get; set; }
    public DateTime EventEndTime { get; set; }
    public DateTime? SetupStartTime { get; set; }
    public DateTime? CleanupEndTime { get; set; }
    public int? MaxAttendees { get; set; }
    public int? EstimatedAttendees { get; set; }
    public bool RequiresVolunteers { get; set; } = false;
    public bool RequiresSetup { get; set; } = false;
    public bool RequiresCleanup { get; set; } = false;
    public string? Notes { get; set; }
    public string? PublicInstructions { get; set; }
    public string? WeatherBackupPlan { get; set; }
    
    public int SchoolYearId { get; set; }
    public int EventTypeId { get; set; }
}

public class EventCoordinatorDto
{
    public bool HasCoordinator { get; set; }
    public string? CoordinatorId { get; set; }
    public string? CoordinatorName { get; set; }
    public string? CoordinatorEmail { get; set; }
}

public class EventTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int Size { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string ColorClass { get; set; } = string.Empty;
}

public class SchoolYearDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
