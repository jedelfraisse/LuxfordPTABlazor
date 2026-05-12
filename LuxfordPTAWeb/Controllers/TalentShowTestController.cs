using System.Text.Json;
using LuxfordPTAWeb.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxfordPTAWeb.Controllers;

[ApiController]
[Route("api/talentshow-test")]
[Authorize(Roles = "Admin,BoardMember")]
public class TalentShowTestController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public TalentShowTestController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet("shows")]
    public ActionResult<IEnumerable<string>> GetShowKeys()
    {
        var rehearsalPath = GetRehearsalDirectoryPath();
        if (!Directory.Exists(rehearsalPath))
        {
            return Ok(Array.Empty<string>());
        }

        var keys = Directory
            .EnumerateFiles(rehearsalPath, "*.schedule.json", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!.Replace(".schedule", string.Empty, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name)
            .ToList();

        return Ok(keys);
    }

    [HttpGet("shows/{showKey}")]
    public async Task<ActionResult<TalentShowRehearsalPackage>> GetShow(string showKey)
    {
        var normalizedKey = NormalizeShowKey(showKey);
        if (string.IsNullOrWhiteSpace(normalizedKey))
        {
            return BadRequest("Invalid show key.");
        }

        var rehearsalPath = GetRehearsalDirectoryPath();
        var scheduleFile = Path.Combine(rehearsalPath, $"{normalizedKey}.schedule.json");
        var presentationFile = Path.Combine(rehearsalPath, $"{normalizedKey}.presentation.json");

        if (!System.IO.File.Exists(scheduleFile))
        {
            return NotFound($"Schedule file '{normalizedKey}.schedule.json' not found.");
        }

        var scheduleJson = await System.IO.File.ReadAllTextAsync(scheduleFile);
        var schedule = JsonSerializer.Deserialize<TalentShowRehearsalPackage>(scheduleJson, JsonOptions);
        if (schedule == null)
        {
            return BadRequest("Schedule file could not be parsed.");
        }

        if (System.IO.File.Exists(presentationFile))
        {
            var presentationJson = await System.IO.File.ReadAllTextAsync(presentationFile);
            var presentation = JsonSerializer.Deserialize<TalentShowPresentationSettings>(presentationJson, JsonOptions);
            if (presentation != null)
            {
                schedule.Presentation = presentation;
            }
        }

        if (string.IsNullOrWhiteSpace(schedule.SessionCode))
        {
            schedule.SessionCode = BuildFallbackSessionCode(normalizedKey);
        }

        return Ok(schedule);
    }

    private string GetRehearsalDirectoryPath()
    {
        return Path.Combine(_environment.WebRootPath, "events", "rehearsals");
    }

    private static string NormalizeShowKey(string showKey)
    {
        return new string(showKey.Trim().ToLowerInvariant()
            .Where(c => char.IsLetterOrDigit(c) || c == '-')
            .ToArray());
    }

    private static string BuildFallbackSessionCode(string key)
    {
        var compact = new string(key.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        return compact.Length <= 6 ? compact : compact[..6];
    }
}
