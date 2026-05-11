using LuxfordPTAWeb.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuxfordPTAWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace LuxfordPTAWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Trusted")]
    public class BugReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public BugReportsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: api/bugreports?pageRoute=...
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? pageRoute)
        {
            var query = _db.BugReports.AsQueryable();
            if (!string.IsNullOrWhiteSpace(pageRoute))
                query = query.Where(b => b.PageRoute == pageRoute);
            var reports = await query.OrderByDescending(b => b.SubmittedAt).ToListAsync();
            return Ok(reports);
        }

        // GET: api/bugreports/count?pageRoute=...
        [HttpGet("count")]
        public async Task<IActionResult> GetCount([FromQuery] string? pageRoute)
        {
            var count = await _db.BugReports.CountAsync(b => b.PageRoute == pageRoute);
            return Ok(count);
        }

        // GET: api/bugreports/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var reports = await _db.BugReports.OrderByDescending(b => b.SubmittedAt).ToListAsync();
            return Ok(reports);
        }

        // POST: api/bugreports
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] BugReport report)
        {
            report.SubmittedAt = DateTime.UtcNow;
            report.IsResolved = false;
            _db.BugReports.Add(report);
            await _db.SaveChangesAsync();
            return Ok(report);
        }

        // PUT: api/bugreports/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] BugReport updated)
        {
            var report = await _db.BugReports.FindAsync(id);
            if (report == null) return NotFound();

            // Only allow editing certain fields
            report.Status = updated.Status;
            report.Description = updated.Description;
            report.Suggestion = updated.Suggestion;
            // Optionally allow IsResolved to be set here if needed

            await _db.SaveChangesAsync();
            return Ok(report);
        }

        // PUT: api/bugreports/{id}/resolve
        [HttpPut("{id}/resolve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Resolve(int id)
        {
            var report = await _db.BugReports.FindAsync(id);
            if (report == null) return NotFound();
            report.IsResolved = true;
            await _db.SaveChangesAsync();
            return Ok(report);
        }
    }
}
