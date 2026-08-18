using System.Text;
using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Services;
using LuxfordPTAWeb.Shared.DTOs;
using LuxfordPTAWeb.Shared.Enums;
using LuxfordPTAWeb.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuxfordPTAWeb.Controllers;

// Membership records contain PII (name, email, phone) and are internal-only per spec —
// every action requires Admin/BoardMember, with no anonymous fallback like SchoolYearsController has.
[ApiController]
[Route("api/membership")]
[Authorize(Roles = "Admin,BoardMember")]
public class MembershipController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IMembershipCsvImportService _importService;
    private readonly IMembershipMilestoneService _milestoneService;

    public MembershipController(ApplicationDbContext db, IMembershipCsvImportService importService, IMembershipMilestoneService milestoneService)
    {
        _db = db;
        _importService = importService;
        _milestoneService = milestoneService;
    }

    [HttpGet("{schoolYearId:int}")]
    public async Task<ActionResult<IEnumerable<MembershipRecord>>> GetForYear(
        int schoolYearId, [FromQuery] string? memberType, [FromQuery] string? teacherName,
        [FromQuery] MembershipStatus? status, [FromQuery] string? paymentType)
    {
        var query = _db.MembershipRecords.Where(mr => mr.SchoolYearId == schoolYearId);

        if (!string.IsNullOrWhiteSpace(memberType))
            query = query.Where(mr => mr.MemberType == memberType);
        if (!string.IsNullOrWhiteSpace(teacherName))
            query = query.Where(mr => mr.TeacherName == teacherName);
        if (status.HasValue)
            query = query.Where(mr => mr.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(paymentType))
            query = query.Where(mr => mr.PaymentType == paymentType);

        return await query.OrderBy(mr => mr.LastName).ThenBy(mr => mr.FirstName).ToListAsync();
    }

    // Aggregate counts only (no PII) — the public /membership-drive page needs this to show live stats.
    [HttpGet("{schoolYearId:int}/stats")]
    [AllowAnonymous]
    public async Task<ActionResult<MembershipStatsDTO>> GetStats(int schoolYearId)
    {
        var exists = await _db.SchoolYears.AnyAsync(sy => sy.Id == schoolYearId);
        if (!exists) return NotFound("School year not found.");

        return await _milestoneService.GetStatsAsync(schoolYearId);
    }

    [HttpGet("{schoolYearId:int}/export")]
    public async Task<IActionResult> Export(int schoolYearId)
    {
        var records = await _db.MembershipRecords
            .Where(mr => mr.SchoolYearId == schoolYearId)
            .OrderBy(mr => mr.LastName).ThenBy(mr => mr.FirstName)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("FirstName,LastName,Email,PhoneNumber,JoinDate,MemberType,Price,Status,PaymentType,TeacherName");
        foreach (var r in records)
        {
            sb.AppendLine(string.Join(",",
                CsvField(r.FirstName), CsvField(r.LastName), CsvField(r.Email), CsvField(r.PhoneNumber),
                r.JoinDate.ToString("yyyy-MM-dd"), CsvField(r.MemberType), r.Price.ToString("0.00"),
                CsvField(r.Status.ToString()), CsvField(r.PaymentType), CsvField(r.TeacherName ?? "")));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"membership-{schoolYearId}.csv");
    }

    private static string CsvField(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    [HttpPost]
    public async Task<ActionResult<MembershipRecord>> Create([FromBody] MembershipRecordDTO dto)
    {
        var schoolYearExists = await _db.SchoolYears.AnyAsync(sy => sy.Id == dto.SchoolYearId);
        if (!schoolYearExists) return BadRequest("Selected school year does not exist.");
        if (string.IsNullOrWhiteSpace(dto.Email) && string.IsNullOrWhiteSpace(dto.PhoneNumber))
            return BadRequest("Either an email or a phone number is required.");

        var record = new MembershipRecord
        {
            Id = Guid.NewGuid(),
            SchoolYearId = dto.SchoolYearId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            JoinDate = dto.JoinDate,
            MemberType = dto.MemberType,
            Price = dto.Price,
            Status = dto.Status,
            PaymentType = dto.PaymentType,
            TeacherName = dto.TeacherName,
            IsStaff = dto.MemberType.Contains("staff", StringComparison.OrdinalIgnoreCase)
                      || dto.MemberType.Contains("teacher", StringComparison.OrdinalIgnoreCase),
            CreatedAt = DateTime.UtcNow
        };

        _db.MembershipRecords.Add(record);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetForYear), new { schoolYearId = record.SchoolYearId }, record);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] MembershipRecordDTO dto)
    {
        var record = await _db.MembershipRecords.FindAsync(id);
        if (record == null) return NotFound();
        if (string.IsNullOrWhiteSpace(dto.Email) && string.IsNullOrWhiteSpace(dto.PhoneNumber))
            return BadRequest("Either an email or a phone number is required.");

        record.FirstName = dto.FirstName;
        record.LastName = dto.LastName;
        record.Email = dto.Email;
        record.PhoneNumber = dto.PhoneNumber;
        record.JoinDate = dto.JoinDate;
        record.MemberType = dto.MemberType;
        record.Price = dto.Price;
        record.Status = dto.Status;
        record.PaymentType = dto.PaymentType;
        record.TeacherName = dto.TeacherName;
        record.IsStaff = dto.MemberType.Contains("staff", StringComparison.OrdinalIgnoreCase)
                          || dto.MemberType.Contains("teacher", StringComparison.OrdinalIgnoreCase);
        record.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var record = await _db.MembershipRecords.FindAsync(id);
        if (record == null) return NotFound();

        _db.MembershipRecords.Remove(record);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("import/preview")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<MembershipImportPreviewDTO>> ImportPreview(IFormFile file, [FromForm] int schoolYearId)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        var schoolYearExists = await _db.SchoolYears.AnyAsync(sy => sy.Id == schoolYearId);
        if (!schoolYearExists) return BadRequest("Selected school year does not exist.");

        await using var stream = file.OpenReadStream();
        return await _importService.ParsePreviewAsync(stream, schoolYearId);
    }

    [HttpPost("import/commit")]
    public async Task<ActionResult<MembershipImportResultDTO>> ImportCommit([FromBody] MembershipImportCommitDTO commit)
    {
        return await _importService.CommitAsync(commit);
    }
}
