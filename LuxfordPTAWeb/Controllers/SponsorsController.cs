using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class SponsorsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public SponsorsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Sponsor>>> Get()
    {
        var sponsors = await _db.Sponsors
            .OrderBy(s => s.Name)
            .ToListAsync();

        return Ok(sponsors);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<ActionResult<Sponsor>> Get(int id)
    {
        var sponsor = await _db.Sponsors.FindAsync(id);
        if (sponsor == null)
        {
            return NotFound();
        }

        return Ok(sponsor);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<ActionResult<Sponsor>> Post([FromBody] Sponsor sponsor)
    {
        sponsor.Name = sponsor.Name.Trim();
        sponsor.LogoUrl = sponsor.LogoUrl?.Trim() ?? string.Empty;
        sponsor.WebsiteUrl = sponsor.WebsiteUrl?.Trim() ?? string.Empty;

        _db.Sponsors.Add(sponsor);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = sponsor.Id }, sponsor);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Put(int id, [FromBody] Sponsor updatedSponsor)
    {
        var sponsor = await _db.Sponsors.FindAsync(id);
        if (sponsor == null)
        {
            return NotFound();
        }

        sponsor.Name = updatedSponsor.Name.Trim();
        sponsor.LogoUrl = updatedSponsor.LogoUrl?.Trim() ?? string.Empty;
        sponsor.WebsiteUrl = updatedSponsor.WebsiteUrl?.Trim() ?? string.Empty;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,BoardMember")]
    public async Task<IActionResult> Delete(int id)
    {
        var sponsor = await _db.Sponsors
            .Include(s => s.SponsorAssignments)
            .Include(s => s.MainEvents)
            .Include(s => s.OtherEvents)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sponsor == null)
        {
            return NotFound();
        }

        if (sponsor.SponsorAssignments.Any() || sponsor.MainEvents.Any() || sponsor.OtherEvents.Any())
        {
            return BadRequest("Cannot delete a sponsor that is assigned to events or sponsorship levels.");
        }

        _db.Sponsors.Remove(sponsor);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
