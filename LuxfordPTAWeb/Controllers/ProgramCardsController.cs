using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuxfordPTAWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProgramCardsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ProgramCardsController> _logger;

    public ProgramCardsController(ApplicationDbContext db, ILogger<ProgramCardsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // GET: api/programcards
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProgramCard>>> Get()
    {
        try
        {
            var programs = await _db.ProgramCards
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayIndex)
                .ThenBy(p => p.ProgramTitle)
                .ToListAsync();

            return Ok(programs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all program cards");
            return BadRequest(new { Error = ex.Message });
        }
    }

    // GET: api/programcards/slug/reflections
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ProgramCard>> GetBySlug(string slug)
    {
        try
        {
            // Normalize the slug for comparison
            var normalizedSlug = slug.ToLowerInvariant().Trim();

            var program = await _db.ProgramCards
                .Where(p => p.Link.ToLower().Contains(normalizedSlug) || 
                            p.ProgramTitle.ToLower().Replace(" ", "-") == normalizedSlug)
                .FirstOrDefaultAsync();

            if (program == null)
            {
                return NotFound(new { Message = $"Program not found: {slug}" });
            }

            return Ok(program);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting program card by slug {Slug}", slug);
            return BadRequest(new { Error = ex.Message });
        }
    }

    // GET: api/programcards/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ProgramCard>> Get(int id)
    {
        try
        {
            var program = await _db.ProgramCards.FindAsync(id);

            if (program == null)
            {
                return NotFound();
            }

            return Ok(program);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting program card {Id}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    // POST: api/programcards
    [Authorize(Roles = "Admin,BoardMember")]
    [HttpPost]
    public async Task<ActionResult<ProgramCard>> Post([FromBody] ProgramCard program)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _db.ProgramCards.Add(program);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Program card created: {Title} (ID: {Id})", program.ProgramTitle, program.Id);

            return CreatedAtAction(nameof(Get), new { id = program.Id }, program);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating program card");
            return BadRequest(new { Error = ex.Message });
        }
    }

    // PUT: api/programcards/5
    [Authorize(Roles = "Admin,BoardMember")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] ProgramCard program)
    {
        try
        {
            _logger.LogInformation("PUT request received for ProgramCard ID: {Id}", id);
            
            if (id != program.Id)
            {
                _logger.LogWarning("ID mismatch: URL ID={UrlId}, Program ID={ProgramId}", id, program.Id);
                return BadRequest(new { Error = "ID mismatch" });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for ProgramCard ID: {Id}", id);
                return BadRequest(ModelState);
            }

            _db.Entry(program).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation("Program card updated: {Title} (ID: {Id})", program.ProgramTitle, program.Id);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating ProgramCard ID: {Id}", id);
                if (!await _db.ProgramCards.AnyAsync(e => e.Id == id))
                {
                    return NotFound();
                }
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating program card {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ex.Message });
        }
    }

    // DELETE: api/programcards/5
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var program = await _db.ProgramCards.FindAsync(id);
            
            if (program == null)
            {
                return NotFound();
            }

            _db.ProgramCards.Remove(program);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Program card deleted: {Title} (ID: {Id})", program.ProgramTitle, id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting program card {Id}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }

    // PATCH: api/programcards/5/toggle-active
    [Authorize(Roles = "Admin,BoardMember")]
    [HttpPatch("{id}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        try
        {
            var program = await _db.ProgramCards.FindAsync(id);
            
            if (program == null)
            {
                return NotFound();
            }

            program.IsActive = !program.IsActive;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Program card active status toggled: {Title} (ID: {Id}) - Active: {IsActive}", 
                program.ProgramTitle, id, program.IsActive);

            return Ok(new { IsActive = program.IsActive });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling active status for program card {Id}", id);
            return BadRequest(new { Error = ex.Message });
        }
    }
}
