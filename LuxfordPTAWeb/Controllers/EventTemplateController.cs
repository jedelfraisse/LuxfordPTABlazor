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

        template.Name = updatedTemplate.Name;
        template.Description = updatedTemplate.Description;
        template.IsActive = updatedTemplate.IsActive;
        template.EventCatId = updatedTemplate.EventCatId;
        template.EventSubTypeId = updatedTemplate.EventSubTypeId;
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
        var title = string.IsNullOrWhiteSpace(request.TitleOverride)
            ? (string.IsNullOrWhiteSpace(template.DefaultTitle) ? template.Name : template.DefaultTitle)
            : request.TitleOverride.Trim();
        var descriptionMarkdown = request.DescriptionOverride ?? template.DefaultDescriptionMarkdown ?? string.Empty;
        var moreDetailsMarkdown = request.MoreDetailsOverride ?? template.DefaultMoreDetailsMarkdown ?? string.Empty;
        var markdownPipeline = new MarkdownPipelineBuilder().DisableHtml().Build();

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
            EventStartTime = date.AddHours(9),
            EventEndTime = date.AddHours(15),
            RequiresVolunteers = template.RequiresVolunteers,
            RequiresSetup = template.RequiresSetup,
            RequiresCleanup = template.RequiresCleanup,
            SchoolYearId = request.SchoolYearId,
            EventCatId = template.EventCatId,
            EventSubTypeId = template.EventSubTypeId,
            Slug = Event.GenerateSlug(title)
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

        return Ok(eventItem);
    }
}
