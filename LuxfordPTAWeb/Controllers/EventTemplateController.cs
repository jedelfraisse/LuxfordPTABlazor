using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Services;
using LuxfordPTAWeb.Shared.DTOs;
using LuxfordPTAWeb.Shared.Enums;
using LuxfordPTAWeb.Shared.Models;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,BoardMember")]
public class EventTemplateController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;

    public EventTemplateController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IAuditService auditService)
    {
        _db = db;
        _userManager = userManager;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventTemplate>>> Get([FromQuery] bool includeInactive = false)
    {
        var query = _db.EventTemplates
            .Include(t => t.EventCat)
            .Include(t => t.EventCatSub)
            .Include(t => t.SourceEvent)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        var templates = await query
            .OrderBy(t => t.EventCat.DisplayOrder)
            .ThenBy(t => t.Name)
            .ToListAsync();

        return Ok(templates);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventTemplate>> Get(int id)
    {
        var template = await _db.EventTemplates
            .Include(t => t.EventCat)
            .Include(t => t.EventCatSub)
            .Include(t => t.SourceEvent)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (template == null)
        {
            return NotFound();
        }

        return Ok(template);
    }

    [HttpPost]
    public async Task<ActionResult<EventTemplate>> Post([FromBody] EventTemplate template)
    {
        if (template.SourceEventId.HasValue)
        {
            var sourceExists = await _db.Events.AnyAsync(e => e.Id == template.SourceEventId.Value);
            if (!sourceExists)
            {
                return BadRequest("Invalid source event.");
            }
        }

        _db.EventTemplates.Add(template);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = template.Id }, template);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] EventTemplate updatedTemplate)
    {
        var template = await _db.EventTemplates.FindAsync(id);
        if (template == null)
        {
            return NotFound();
        }

        if (updatedTemplate.SourceEventId.HasValue)
        {
            var sourceExists = await _db.Events.AnyAsync(e => e.Id == updatedTemplate.SourceEventId.Value);
            if (!sourceExists)
            {
                return BadRequest("Invalid source event.");
            }
        }

        template.Name = updatedTemplate.Name;
        template.Description = updatedTemplate.Description;
        template.IsActive = updatedTemplate.IsActive;
        template.EventCatId = updatedTemplate.EventCatId;
        template.EventSubTypeId = updatedTemplate.EventSubTypeId;
        template.SourceEventId = updatedTemplate.SourceEventId;
        template.DefaultTitle = updatedTemplate.DefaultTitle;
        template.DefaultLocation = updatedTemplate.DefaultLocation;
        template.DefaultDescriptionMarkdown = updatedTemplate.DefaultDescriptionMarkdown;
        template.DefaultMoreDetailsMarkdown = updatedTemplate.DefaultMoreDetailsMarkdown;
        template.RequiresVolunteers = updatedTemplate.RequiresVolunteers;
        template.RequiresSetup = updatedTemplate.RequiresSetup;
        template.RequiresCleanup = updatedTemplate.RequiresCleanup;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var template = await _db.EventTemplates.FindAsync(id);
        if (template == null)
        {
            return NotFound();
        }

        _db.EventTemplates.Remove(template);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/create-event")]
    public async Task<ActionResult<Event>> CreateEventFromTemplate(int id, [FromBody] CreateEventFromTemplateDTO request)
    {
        var template = await _db.EventTemplates
            .Include(t => t.SourceEvent)
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        if (template == null)
        {
            return NotFound("Template not found or inactive.");
        }

        var schoolYear = await _db.SchoolYears.FindAsync(request.SchoolYearId);
        if (schoolYear == null)
        {
            return BadRequest("Invalid school year.");
        }

        var date = request.Date == default ? DateTime.Today.AddDays(14) : request.Date.Date;
        Event? sourceEvent = null;
        if (template.SourceEventId.HasValue)
        {
            sourceEvent = await _db.Events
                .Include(e => e.EventDays)
                .FirstOrDefaultAsync(e => e.Id == template.SourceEventId.Value);
        }

        var title = string.IsNullOrWhiteSpace(request.TitleOverride)
            ? (string.IsNullOrWhiteSpace(template.DefaultTitle) ? template.Name : template.DefaultTitle)
            : request.TitleOverride.Trim();
        var descriptionMarkdown = request.DescriptionOverride ?? template.DefaultDescriptionMarkdown ?? string.Empty;
        var moreDetailsMarkdown = request.MoreDetailsOverride ?? template.DefaultMoreDetailsMarkdown ?? string.Empty;
        var markdownPipeline = new MarkdownPipelineBuilder().DisableHtml().Build();
        var dayOffset = sourceEvent != null ? date.Subtract(sourceEvent.Date).Days : 0;
        var sourceEventStartTime = sourceEvent != null
            ? sourceEvent.EventStartTime.AddDays(dayOffset)
            : date.AddHours(9);
        var sourceEventEndTime = sourceEvent != null
            ? sourceEvent.EventEndTime.AddDays(dayOffset)
            : date.AddHours(15);

        var eventItem = new Event
        {
            Title = title,
            Date = date,
            Description = descriptionMarkdown,
            DescriptionMarkdown = descriptionMarkdown,
            DescriptionHtml = Markdig.Markdown.ToHtml(descriptionMarkdown, markdownPipeline),
            MoreDetailsMarkdown = moreDetailsMarkdown,
            MoreDetailsHtml = Markdig.Markdown.ToHtml(moreDetailsMarkdown, markdownPipeline),
            Location = request.LocationOverride ?? template.DefaultLocation ?? string.Empty,
            Status = EventStatus.Planning,
            EventStartTime = sourceEventStartTime,
            EventEndTime = sourceEventEndTime,
            RequiresVolunteers = template.RequiresVolunteers,
            RequiresSetup = template.RequiresSetup,
            RequiresCleanup = template.RequiresCleanup,
            SchoolYearId = request.SchoolYearId,
            EventCatId = template.EventCatId,
            EventSubTypeId = template.EventSubTypeId,
            Slug = Event.GenerateSlug(title),
            SourceEventId = sourceEvent?.Id,
            CopyGeneration = sourceEvent != null ? sourceEvent.CopyGeneration + 1 : 0,
            SetupStartTime = sourceEvent?.SetupStartTime?.AddDays(dayOffset),
            CleanupEndTime = sourceEvent?.CleanupEndTime?.AddDays(dayOffset),
            SignupWindowStart = sourceEvent?.SignupWindowStart?.AddDays(dayOffset),
            SignupWindowEnd = sourceEvent?.SignupWindowEnd?.AddDays(dayOffset),
            Notes = sourceEvent?.Notes ?? string.Empty,
            PublicInstructions = sourceEvent?.PublicInstructions ?? string.Empty,
            WeatherBackupPlan = sourceEvent?.WeatherBackupPlan ?? string.Empty,
            MaxAttendees = sourceEvent?.MaxAttendees,
            EstimatedAttendees = sourceEvent?.EstimatedAttendees
        };

        var existingSlug = await _db.Events.AnyAsync(e => e.Slug == eventItem.Slug && e.SchoolYearId == eventItem.SchoolYearId);
        if (existingSlug)
        {
            eventItem.Slug = $"{eventItem.Slug}-{DateTime.UtcNow:MMdd}";
        }

        var currentUser = await _userManager.GetUserAsync(User);
        await _auditService.SetCreationAuditAsync(eventItem, currentUser);

        _db.Events.Add(eventItem);
        await _db.SaveChangesAsync();

        if (sourceEvent?.EventDays.Any() == true)
        {
            foreach (var sourceDay in sourceEvent.EventDays.OrderBy(d => d.DayNumber))
            {
                _db.EventDays.Add(new EventDay
                {
                    EventId = eventItem.Id,
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
                });
            }

            await _db.SaveChangesAsync();
        }

        await _db.Entry(eventItem).Collection(e => e.EventDays).LoadAsync();

        return Ok(eventItem);
    }
}
