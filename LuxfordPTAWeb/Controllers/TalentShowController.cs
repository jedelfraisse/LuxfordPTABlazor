using System.Text.Json;
using System.Text;
using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Hubs;
using LuxfordPTAWeb.Shared.DTOs;
using LuxfordPTAWeb.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LuxfordPTAWeb.Controllers;

[ApiController]
[Route("api/talentshow/{eventId:int}")]
[Authorize(Roles = "Admin,BoardMember")]
public class TalentShowController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IHubContext<TalentShowHub> _hubContext;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private const string TalentShowDirectorControlType = "TalentShowDirectorControl";

    public TalentShowController(ApplicationDbContext db, IHubContext<TalentShowHub> hubContext)
    {
        _db = db;
        _hubContext = hubContext;
    }

    [HttpGet("state")]
    public async Task<ActionResult<TalentShowRealtimeState>> GetState(int eventId)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var session = await GetOrCreateSessionAsync(eventItem);
        var acts = await _db.TalentShowActs
            .Where(a => a.EventId == eventId)
            .OrderBy(a => a.OrderIndex)
            .ToListAsync();
        var votes = await _db.TalentShowVotes
            .Where(v => v.EventId == eventId)
            .OrderBy(v => v.TimestampUtc)
            .ToListAsync();

        return Ok(ToRealtimeState(session, acts, votes));
    }

    [HttpPost("state")]
    public async Task<IActionResult> SaveState(int eventId, [FromBody] TalentShowRealtimeState incomingState)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var session = await GetOrCreateSessionAsync(eventItem);
        var acts = await _db.TalentShowActs
            .Where(a => a.EventId == eventId)
            .OrderBy(a => a.OrderIndex)
            .ToListAsync();

        ApplyRealtimeStateToSession(session, incomingState, acts);
        await SeedActsIfEmptyAsync(eventId, acts, incomingState.Items);

        session.LastUpdated = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("advance")]
    public async Task<ActionResult<TalentShowRealtimeState>> Advance(int eventId)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var session = await GetOrCreateSessionAsync(eventItem);
        var acts = await _db.TalentShowActs
            .Where(a => a.EventId == eventId)
            .OrderBy(a => a.OrderIndex)
            .ToListAsync();

        if (acts.Any() && session.CurrentIndex < acts.Count - 1)
        {
            session.CurrentIndex++;
            session.CurrentActId = acts[session.CurrentIndex].Id;
            session.NextActId = session.CurrentIndex + 1 < acts.Count ? acts[session.CurrentIndex + 1].Id : null;
            if (session.CurrentState == TalentShowLifecycleState.HostTalk)
            {
                session.CurrentState = TalentShowLifecycleState.ActShow;
                session.LiveSubState = TalentShowLiveSubState.ActShow;
            }
        }

        session.LastUpdated = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var votes = await _db.TalentShowVotes
            .Where(v => v.EventId == eventId)
            .OrderBy(v => v.TimestampUtc)
            .ToListAsync();

        return Ok(ToRealtimeState(session, acts, votes));
    }

    [HttpPost("open-voting")]
    public async Task<IActionResult> OpenVoting(int eventId)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var session = await GetOrCreateSessionAsync(eventItem);
        session.VotingOpen = true;
        session.LastUpdated = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("close-voting")]
    public async Task<IActionResult> CloseVoting(int eventId)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var session = await GetOrCreateSessionAsync(eventItem);
        session.VotingOpen = false;
        session.LastUpdated = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("transition-lifecycle")]
    public async Task<ActionResult<TalentShowRealtimeState>> TransitionLifecycleState(int eventId, [FromBody] TalentShowLifecycleTransitionDTO dto)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var session = await GetOrCreateSessionAsync(eventItem);
        var targetState = dto.TargetState?.Trim() ?? string.Empty;

        if (!TalentShowLifecycleState.All.Contains(targetState, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest($"Invalid lifecycle state. Valid states: {string.Join(", ", TalentShowLifecycleState.All)}");
        }

        var normalizedTargetState = TalentShowLifecycleState.All.First(s => s.Equals(targetState, StringComparison.OrdinalIgnoreCase));
        session.CurrentState = normalizedTargetState;
        session.OverallState = NormalizeOverallState(null, normalizedTargetState);
        session.LastUpdated = DateTime.UtcNow;

        if (session.IsLiveMode && (normalizedTargetState == TalentShowLifecycleState.HostTalk || normalizedTargetState == TalentShowLifecycleState.ActShow))
        {
            session.SegmentStartedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        var acts = await _db.TalentShowActs
            .Where(a => a.EventId == eventId)
            .OrderBy(a => a.OrderIndex)
            .ToListAsync();
        var votes = await _db.TalentShowVotes
            .Where(v => v.EventId == eventId)
            .OrderBy(v => v.TimestampUtc)
            .ToListAsync();

        return Ok(ToRealtimeState(session, acts, votes));
    }

    [HttpGet("planning/configure")]
    public async Task<ActionResult<TalentShowPlanningConfigDTO>> GetPlanningConfiguration(int eventId)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var control = await GetTalentShowDirectorControlAsync(eventId);
        if (control == null)
        {
            return Ok(new TalentShowPlanningConfigDTO());
        }

        var settings = ParseDirectorSettings(control.SettingsJson);
        var planning = NormalizePlanningConfig(settings.Planning);
        var globalSetup = NormalizeGlobalSetupConfig(settings.GlobalSetup, eventItem);
        return Ok(NormalizePlanningConfig(ApplyGlobalDefaultsToPlanning(planning, globalSetup)));
    }

    [HttpPut("planning/configure")]
    public async Task<IActionResult> SavePlanningConfiguration(int eventId, [FromBody] TalentShowPlanningConfigDTO dto)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var control = await GetOrCreateTalentShowDirectorControlAsync(eventItem);
        var settings = ParseDirectorSettings(control.SettingsJson);
        settings.Planning = NormalizePlanningConfig(dto);
        control.SettingsJson = JsonSerializer.Serialize(settings);

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("global/configure")]
    public async Task<ActionResult<TalentShowGlobalSetupConfigDTO>> GetGlobalConfiguration(int eventId)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var control = await GetTalentShowDirectorControlAsync(eventId);
        var settings = control == null
            ? new TalentShowDirectorSettings()
            : ParseDirectorSettings(control.SettingsJson);

        var normalized = NormalizeGlobalSetupConfig(settings.GlobalSetup, eventItem);
        normalized.EventTitle = eventItem.Title?.Trim() ?? normalized.EventTitle;
        return Ok(normalized);
    }

    [HttpPut("global/configure")]
    public async Task<IActionResult> SaveGlobalConfiguration(int eventId, [FromBody] TalentShowGlobalSetupConfigDTO dto)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var control = await GetOrCreateTalentShowDirectorControlAsync(eventItem);
        var settings = ParseDirectorSettings(control.SettingsJson);
        var normalizedGlobalSetup = NormalizeGlobalSetupConfig(dto, eventItem);
        settings.GlobalSetup = normalizedGlobalSetup;
        eventItem.Title = normalizedGlobalSetup.EventTitle;
        control.SettingsJson = JsonSerializer.Serialize(settings);

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("signups/configure")]
    public async Task<ActionResult<TalentShowSignupSettingsDTO>> GetSignupConfiguration(int eventId)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var control = await GetTalentShowDirectorControlAsync(eventId);
        var settings = control == null
            ? new TalentShowDirectorSettings()
            : ParseDirectorSettings(control.SettingsJson);

        var planningForDefaults = NormalizePlanningConfig(ApplyGlobalDefaultsToPlanning(
            NormalizePlanningConfig(settings.Planning),
            NormalizeGlobalSetupConfig(settings.GlobalSetup, eventItem)));

        return Ok(NormalizeSignupSettings(settings.Signups, planningForDefaults, eventItem));
    }

    [AllowAnonymous]
    [HttpGet("public/signups/configure")]
    public async Task<ActionResult<TalentShowSignupSettingsDTO>> GetPublicSignupConfiguration(int eventId)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var settings = await GetNormalizedSignupSettingsAsync(eventItem);
        if (!settings.IsPublished)
        {
            return NotFound("Sign-ups are not published for this event.");
        }

        return Ok(settings);
    }

    [HttpPut("signups/configure")]
    public async Task<IActionResult> SaveSignupConfiguration(int eventId, [FromBody] TalentShowSignupSettingsDTO dto)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var control = await GetOrCreateTalentShowDirectorControlAsync(eventItem);
        var settings = ParseDirectorSettings(control.SettingsJson);
        var planningForDefaults = NormalizePlanningConfig(ApplyGlobalDefaultsToPlanning(
            NormalizePlanningConfig(settings.Planning),
            NormalizeGlobalSetupConfig(settings.GlobalSetup, eventItem)));
        settings.Signups = NormalizeSignupSettings(dto, planningForDefaults, eventItem);
        control.SettingsJson = JsonSerializer.Serialize(settings);

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("signups/submissions")]
    public async Task<ActionResult<IEnumerable<TalentShowSignup>>> GetSignupSubmissions(int eventId, [FromQuery] string? status)
    {
        var eventExists = await _db.Events.AnyAsync(e => e.Id == eventId);
        if (!eventExists)
        {
            return NotFound("Event not found.");
        }

        var query = _db.TalentShowSignups
            .Where(s => s.EventId == eventId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(s => s.Status == status.Trim());
        }

        var signups = await query
            .OrderByDescending(s => s.CreatedAtUtc)
            .ToListAsync();

        return Ok(signups);
    }

    [HttpPost("signups/submissions")]
    public async Task<ActionResult<TalentShowSignup>> CreateSignupSubmission(int eventId, [FromBody] TalentShowSignupUpsertDTO dto)
    {
        var eventExists = await _db.Events.AnyAsync(e => e.Id == eventId);
        if (!eventExists)
        {
            return NotFound("Event not found.");
        }

        var signup = new TalentShowSignup
        {
            EventId = eventId,
            PerformerNames = dto.PerformerNames?.Trim() ?? string.Empty,
            Grade = dto.Grade?.Trim() ?? string.Empty,
            Teacher = dto.Teacher?.Trim() ?? string.Empty,
            ActTitle = dto.ActTitle?.Trim() ?? string.Empty,
            ActDescription = dto.ActDescription?.Trim() ?? string.Empty,
            ContactEmail = dto.ContactEmail?.Trim() ?? string.Empty,
            ContactPhone = dto.ContactPhone?.Trim() ?? string.Empty,
            SpecialRequirements = dto.SpecialRequirements?.Trim() ?? string.Empty,
            MediaUpload = dto.MediaUpload?.Trim() ?? string.Empty,
            PickupAdult = dto.PickupAdult?.Trim() ?? string.Empty,
            MusicUrl = dto.MusicUrl?.Trim() ?? string.Empty,
            Status = TalentShowSignupStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _db.TalentShowSignups.Add(signup);
        await _db.SaveChangesAsync();
        return Ok(signup);
    }

    [AllowAnonymous]
    [HttpPost("public/signups/submissions")]
    public async Task<ActionResult<TalentShowSignup>> CreatePublicSignupSubmission(int eventId, [FromBody] TalentShowSignupUpsertDTO dto)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var settings = await GetNormalizedSignupSettingsAsync(eventItem);
        if (!settings.IsPublished)
        {
            return BadRequest("Sign-ups are not published for this event.");
        }

        if (!IsSignupWindowOpen(settings))
        {
            return BadRequest("Sign-up window is currently closed.");
        }

        var signup = new TalentShowSignup
        {
            EventId = eventId,
            PerformerNames = dto.PerformerNames?.Trim() ?? string.Empty,
            Grade = dto.Grade?.Trim() ?? string.Empty,
            Teacher = dto.Teacher?.Trim() ?? string.Empty,
            ActTitle = dto.ActTitle?.Trim() ?? string.Empty,
            ActDescription = dto.ActDescription?.Trim() ?? string.Empty,
            ContactEmail = dto.ContactEmail?.Trim() ?? string.Empty,
            ContactPhone = dto.ContactPhone?.Trim() ?? string.Empty,
            SpecialRequirements = dto.SpecialRequirements?.Trim() ?? string.Empty,
            MediaUpload = dto.MediaUpload?.Trim() ?? string.Empty,
            PickupAdult = dto.PickupAdult?.Trim() ?? string.Empty,
            MusicUrl = dto.MusicUrl?.Trim() ?? string.Empty,
            Status = TalentShowSignupStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _db.TalentShowSignups.Add(signup);
        await _db.SaveChangesAsync();
        return Ok(signup);
    }

    [HttpPut("signups/submissions/{signupId:int}")]
    public async Task<IActionResult> UpdateSignupSubmission(int eventId, int signupId, [FromBody] TalentShowSignupUpsertDTO dto)
    {
        var signup = await _db.TalentShowSignups.FirstOrDefaultAsync(s => s.EventId == eventId && s.Id == signupId);
        if (signup == null)
        {
            return NotFound("Sign-up not found.");
        }

        signup.PerformerNames = dto.PerformerNames?.Trim() ?? string.Empty;
        signup.Grade = dto.Grade?.Trim() ?? string.Empty;
        signup.Teacher = dto.Teacher?.Trim() ?? string.Empty;
        signup.ActTitle = dto.ActTitle?.Trim() ?? string.Empty;
        signup.ActDescription = dto.ActDescription?.Trim() ?? string.Empty;
        signup.ContactEmail = dto.ContactEmail?.Trim() ?? string.Empty;
        signup.ContactPhone = dto.ContactPhone?.Trim() ?? string.Empty;
        signup.SpecialRequirements = dto.SpecialRequirements?.Trim() ?? string.Empty;
        signup.MediaUpload = dto.MediaUpload?.Trim() ?? string.Empty;
        signup.PickupAdult = dto.PickupAdult?.Trim() ?? string.Empty;
        signup.MusicUrl = dto.MusicUrl?.Trim() ?? string.Empty;
        signup.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("signups/submissions/{signupId:int}/review")]
    public async Task<ActionResult<TalentShowSignup>> ReviewSignupSubmission(int eventId, int signupId, [FromBody] TalentShowSignupReviewDecisionDTO dto)
    {
        var signup = await _db.TalentShowSignups.FirstOrDefaultAsync(s => s.EventId == eventId && s.Id == signupId);
        if (signup == null)
        {
            return NotFound("Sign-up not found.");
        }

        var normalizedAction = NormalizeSignupAction(dto.Action);
        if (string.IsNullOrWhiteSpace(normalizedAction))
        {
            return BadRequest($"Invalid review action. Valid actions: {string.Join(", ", TalentShowSignupStatus.All)}");
        }

        signup.Status = normalizedAction;
        signup.ReviewReason = dto.Reason?.Trim() ?? string.Empty;
        signup.AllowResubmission = dto.AllowResubmission;
        signup.TryOutSession = dto.TryOutSession?.Trim() ?? string.Empty;
        signup.UpdatedAtUtc = DateTime.UtcNow;

        if (normalizedAction == TalentShowSignupStatus.ApprovedDirect)
        {
            await EnsureActForSignupAsync(eventId, signup);
        }
        else if (normalizedAction == TalentShowSignupStatus.InvitedToTryOuts)
        {
            await EnsureTryOutEntryForSignupAsync(eventId, signup, dto.TryOutSession);
        }

        await _db.SaveChangesAsync();
        return Ok(signup);
    }

    [HttpGet("tryouts/configure")]
    [HttpGet("tryouts/configuration")]
    public async Task<ActionResult<TalentShowTryOutsConfigDTO>> GetTryOutsConfiguration(int eventId)
    {
        var eventExists = await _db.Events.AnyAsync(e => e.Id == eventId);
        if (!eventExists)
        {
            return NotFound("Event not found.");
        }

        var control = await GetTalentShowDirectorControlAsync(eventId);
        var settings = control == null
            ? new TalentShowDirectorSettings()
            : ParseDirectorSettings(control.SettingsJson);

        return Ok(NormalizeTryOutsConfig(settings.TryOuts));
    }

    [HttpPut("tryouts/configure")]
    [HttpPut("tryouts/configuration")]
    public async Task<IActionResult> SaveTryOutsConfiguration(int eventId, [FromBody] TalentShowTryOutsConfigDTO dto)
    {
        var eventItem = await _db.Events
            .Include(e => e.EventDays)
            .FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var control = await GetOrCreateTalentShowDirectorControlAsync(eventItem);
        var settings = ParseDirectorSettings(control.SettingsJson);
        settings.TryOuts = NormalizeTryOutsConfig(dto);
        control.SettingsJson = JsonSerializer.Serialize(settings);

        // Sync sessions to EventDays
        await SyncTryOutSessionsToEventDaysAsync(eventId, eventItem, settings.TryOuts.Sessions);

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("tryouts/entries")]
    public async Task<ActionResult<IEnumerable<TalentShowTryOutEntry>>> GetTryOutEntries(int eventId)
    {
        var eventExists = await _db.Events.AnyAsync(e => e.Id == eventId);
        if (!eventExists)
        {
            return NotFound("Event not found.");
        }

        var entries = await _db.TalentShowTryOutEntries
            .Where(te => te.EventId == eventId)
            .OrderBy(te => te.SlotTime ?? DateTime.MaxValue)
            .ThenBy(te => te.CreatedAtUtc)
            .ToListAsync();

        return Ok(entries);
    }

    [HttpPost("tryouts/entries")]
    public async Task<ActionResult<TalentShowTryOutEntry>> CreateTryOutEntry(int eventId, [FromBody] TalentShowTryOutEntryUpsertDTO dto)
    {
        var eventExists = await _db.Events.AnyAsync(e => e.Id == eventId);
        if (!eventExists)
        {
            return NotFound("Event not found.");
        }

        var entry = new TalentShowTryOutEntry
        {
            EventId = eventId,
            SignupId = dto.SignupId,
            PerformerNames = dto.PerformerNames?.Trim() ?? string.Empty,
            ActTitle = dto.ActTitle?.Trim() ?? string.Empty,
            MusicUrl = dto.MusicUrl?.Trim() ?? string.Empty,
            PickupAdult = dto.PickupAdult?.Trim() ?? string.Empty,
            SlotTime = dto.SlotTime,
            CheckInTimestamp = dto.CheckInTimestamp,
            SessionLabel = dto.SessionLabel?.Trim() ?? string.Empty,
            Notes = dto.Notes?.Trim() ?? string.Empty,
            ScoresJson = JsonSerializer.Serialize(dto.Scores ?? [], JsonOptions),
            JudgeCompletionsJson = JsonSerializer.Serialize(dto.JudgeCompletions ?? [], JsonOptions),
            Status = NormalizeTryOutEntryStatus(dto.Status),
            Selected = dto.Selected,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        if (entry.Status == TalentShowTryOutEntryStatus.StandBy && entry.CheckInTimestamp == null)
        {
            entry.CheckInTimestamp = DateTime.UtcNow;
        }

        if (entry.Status == TalentShowTryOutEntryStatus.OnDeck)
        {
            await ClearOtherOnDeckEntriesAsync(eventId, entry.SessionLabel, null);
        }

        if (entry.Status == TalentShowTryOutEntryStatus.Approved)
        {
            entry.Selected = true;
            await EnsureActForTryOutEntryAsync(eventId, entry);
            await EnsureSegmentStubForTryOutEntryAsync(eventId, entry);
        }
        else if (entry.Status == TalentShowTryOutEntryStatus.Rejected)
        {
            entry.Selected = false;
        }

        _db.TalentShowTryOutEntries.Add(entry);
        await _db.SaveChangesAsync();
        return Ok(entry);
    }

    [HttpPut("tryouts/entries/{entryId:int}")]
    public async Task<IActionResult> UpdateTryOutEntry(int eventId, int entryId, [FromBody] TalentShowTryOutEntryUpsertDTO dto)
    {
        var entry = await _db.TalentShowTryOutEntries.FirstOrDefaultAsync(te => te.EventId == eventId && te.Id == entryId);
        if (entry == null)
        {
            return NotFound("Try-out entry not found.");
        }

        entry.SlotTime = dto.SlotTime;
        entry.CheckInTimestamp = dto.CheckInTimestamp;
        entry.SessionLabel = dto.SessionLabel?.Trim() ?? string.Empty;
        entry.Notes = dto.Notes?.Trim() ?? string.Empty;
        entry.MusicUrl = dto.MusicUrl?.Trim() ?? string.Empty;
        entry.PickupAdult = dto.PickupAdult?.Trim() ?? string.Empty;
        entry.ScoresJson = JsonSerializer.Serialize(dto.Scores ?? [], JsonOptions);
        entry.JudgeCompletionsJson = JsonSerializer.Serialize(dto.JudgeCompletions ?? [], JsonOptions);
        entry.Status = NormalizeTryOutEntryStatus(dto.Status);
        entry.Selected = dto.Selected;
        entry.UpdatedAtUtc = DateTime.UtcNow;

        if (entry.Status == TalentShowTryOutEntryStatus.StandBy && entry.CheckInTimestamp == null)
        {
            entry.CheckInTimestamp = DateTime.UtcNow;
        }

        if (entry.Status == TalentShowTryOutEntryStatus.OnDeck)
        {
            await ClearOtherOnDeckEntriesAsync(eventId, entry.SessionLabel, entry.Id);
        }

        if (entry.Status == TalentShowTryOutEntryStatus.Approved)
        {
            entry.Selected = true;
            await EnsureActForTryOutEntryAsync(eventId, entry);
            await EnsureSegmentStubForTryOutEntryAsync(eventId, entry);
        }
        else if (entry.Status == TalentShowTryOutEntryStatus.Rejected)
        {
            entry.Selected = false;
        }
        else if (entry.Selected)
        {
            await EnsureActForTryOutEntryAsync(eventId, entry);
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("tryouts/pdfs/parent-signout")]
    public async Task<IActionResult> GetTryOutParentSignOutPdf(int eventId, [FromQuery] string? sessionId)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var entries = await GetStandByEntriesForPdfAsync(eventId, sessionId);
        var lines = entries
            .Select(entry => $"{entry.PerformerNames} | ________________________________ | ________________________________")
            .ToList();

        if (!lines.Any())
        {
            lines.Add("No currently checked-in StandBy students.");
        }

        var pdf = BuildSimplePdf(
            "Parent Sign-Out Sheet",
            $"{eventItem.Title} | Session: {GetSessionLabel(sessionId)} | Generated: {DateTime.Now:g}",
            lines);

        var fileName = $"tryouts-parent-signout-{eventId}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        return File(pdf, "application/pdf", fileName);
    }

    [HttpGet("tryouts/pdfs/authorized-pickup")]
    public async Task<IActionResult> GetTryOutAuthorizedPickupPdf(int eventId, [FromQuery] string? sessionId)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var entries = await GetStandByEntriesForPdfAsync(eventId, sessionId);
        var lines = entries
            .Select(entry => $"{entry.PerformerNames} | {(string.IsNullOrWhiteSpace(entry.PickupAdult) ? "(not provided)" : entry.PickupAdult)}")
            .ToList();

        if (!lines.Any())
        {
            lines.Add("No currently checked-in StandBy students.");
        }

        var pdf = BuildSimplePdf(
            "Authorized Pickup List (Staff Only)",
            $"{eventItem.Title} | Session: {GetSessionLabel(sessionId)} | Generated: {DateTime.Now:g}",
            lines);

        var fileName = $"tryouts-authorized-pickup-{eventId}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        return File(pdf, "application/pdf", fileName);
    }

    [HttpGet("acts")]
    public async Task<ActionResult<IEnumerable<TalentShowAct>>> GetActs(int eventId)
    {
        var exists = await _db.Events.AnyAsync(e => e.Id == eventId);
        if (!exists)
        {
            return NotFound("Event not found.");
        }

        var acts = await _db.TalentShowActs
            .Where(a => a.EventId == eventId)
            .OrderBy(a => a.OrderIndex)
            .ToListAsync();
        return Ok(acts);
    }

    [HttpPost("acts")]
    public async Task<ActionResult<TalentShowAct>> CreateAct(int eventId, [FromBody] TalentShowActUpsertDTO dto)
    {
        var exists = await _db.Events.AnyAsync(e => e.Id == eventId);
        if (!exists)
        {
            return NotFound("Event not found.");
        }

        var act = new TalentShowAct
        {
            EventId = eventId,
            PerformerName = dto.PerformerName?.Trim() ?? string.Empty,
            Title = dto.Title?.Trim() ?? string.Empty,
            Category = dto.Category?.Trim() ?? string.Empty,
            DurationSeconds = dto.DurationSeconds <= 0 ? 60 : dto.DurationSeconds,
            OrderIndex = dto.OrderIndex <= 0 ? await NextOrderIndexAsync(eventId) : dto.OrderIndex,
            IntroLine = dto.IntroLine?.Trim() ?? string.Empty,
            OutroLine = dto.OutroLine?.Trim() ?? string.Empty,
            MediaFilePath = dto.MediaFilePath?.Trim() ?? string.Empty,
            Notes = dto.Notes?.Trim() ?? string.Empty,
            SelectedForShow = dto.SelectedForShow
        };

        _db.TalentShowActs.Add(act);
        await _db.SaveChangesAsync();
        return Ok(act);
    }

    [HttpPut("acts/{actId:int}")]
    public async Task<IActionResult> UpdateAct(int eventId, int actId, [FromBody] TalentShowActUpsertDTO dto)
    {
        var act = await _db.TalentShowActs.FirstOrDefaultAsync(a => a.EventId == eventId && a.Id == actId);
        if (act == null)
        {
            return NotFound("Act not found.");
        }

        act.PerformerName = dto.PerformerName?.Trim() ?? string.Empty;
        act.Title = dto.Title?.Trim() ?? string.Empty;
        act.Category = dto.Category?.Trim() ?? string.Empty;
        act.DurationSeconds = dto.DurationSeconds <= 0 ? act.DurationSeconds : dto.DurationSeconds;
        act.OrderIndex = dto.OrderIndex <= 0 ? act.OrderIndex : dto.OrderIndex;
        act.IntroLine = dto.IntroLine?.Trim() ?? string.Empty;
        act.OutroLine = dto.OutroLine?.Trim() ?? string.Empty;
        act.MediaFilePath = dto.MediaFilePath?.Trim() ?? string.Empty;
        act.Notes = dto.Notes?.Trim() ?? string.Empty;
        act.SelectedForShow = dto.SelectedForShow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("acts/{actId:int}")]
    public async Task<IActionResult> DeleteAct(int eventId, int actId)
    {
        var act = await _db.TalentShowActs.FirstOrDefaultAsync(a => a.EventId == eventId && a.Id == actId);
        if (act == null)
        {
            return NotFound("Act not found.");
        }

        _db.TalentShowActs.Remove(act);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("acts/reorder")]
    public async Task<IActionResult> ReorderActs(int eventId, [FromBody] TalentShowReorderActsDTO dto)
    {
        var acts = await _db.TalentShowActs
            .Where(a => a.EventId == eventId)
            .ToListAsync();

        if (!acts.Any())
        {
            return NoContent();
        }

        var order = 1;
        foreach (var id in dto.OrderedActIds)
        {
            var act = acts.FirstOrDefault(a => a.Id == id);
            if (act == null)
            {
                continue;
            }

            act.OrderIndex = order++;
        }

        foreach (var remainingAct in acts.Where(a => !dto.OrderedActIds.Contains(a.Id)).OrderBy(a => a.OrderIndex))
        {
            remainingAct.OrderIndex = order++;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("vote")]
    public async Task<IActionResult> SubmitVote(int eventId, [FromBody] TalentShowVoteSubmission vote)
    {
        var exists = await _db.Events.AnyAsync(e => e.Id == eventId);
        if (!exists)
        {
            return NotFound("Event not found.");
        }

        var act = await _db.TalentShowActs
            .Where(a => a.EventId == eventId && a.OrderIndex == vote.ActOrder)
            .FirstOrDefaultAsync();

        var safeVote = new TalentShowVote
        {
            EventId = eventId,
            ActId = act?.Id,
            DeviceOrUserId = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            VoterName = string.IsNullOrWhiteSpace(vote.VoterName) ? "Anonymous" : vote.VoterName.Trim(),
            IsJudge = vote.IsJudge,
            TalentScore = ClampScore(vote.TalentScore),
            StagePresenceScore = ClampScore(vote.StagePresenceScore),
            CreativityScore = ClampScore(vote.CreativityScore),
            CrowdEngagementScore = ClampScore(vote.CrowdEngagementScore),
            TimestampUtc = DateTime.UtcNow
        };
        safeVote.TotalScore = safeVote.TalentScore + safeVote.StagePresenceScore + safeVote.CreativityScore + safeVote.CrowdEngagementScore;

        _db.TalentShowVotes.Add(safeVote);
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("results")]
    public async Task<ActionResult<IEnumerable<TalentShowVoteResultRowDTO>>> GetResults(int eventId)
    {
        var exists = await _db.Events.AnyAsync(e => e.Id == eventId);
        if (!exists)
        {
            return NotFound("Event not found.");
        }

        var rows = await _db.TalentShowActs
            .Where(a => a.EventId == eventId)
            .OrderBy(a => a.OrderIndex)
            .Select(a => new TalentShowVoteResultRowDTO
            {
                ActId = a.Id,
                PerformerName = a.PerformerName,
                Title = a.Title,
                JudgeCount = _db.TalentShowVotes.Count(v => v.EventId == eventId && v.ActId == a.Id && v.IsJudge),
                AudienceCount = _db.TalentShowVotes.Count(v => v.EventId == eventId && v.ActId == a.Id && !v.IsJudge),
                JudgeAverageTotal = _db.TalentShowVotes.Where(v => v.EventId == eventId && v.ActId == a.Id && v.IsJudge).Select(v => (double?)v.TotalScore).Average() ?? 0d,
                AudienceAverageTotal = _db.TalentShowVotes.Where(v => v.EventId == eventId && v.ActId == a.Id && !v.IsJudge).Select(v => (double?)v.TotalScore).Average() ?? 0d
            })
            .ToListAsync();

        foreach (var row in rows)
        {
            row.CombinedAverageTotal = row.JudgeCount == 0 && row.AudienceCount == 0
                ? 0d
                : (row.JudgeAverageTotal + row.AudienceAverageTotal) / (row.JudgeCount > 0 && row.AudienceCount > 0 ? 2d : 1d);
        }

        return Ok(rows);
    }

    private async Task<TalentShowSessionState> GetOrCreateSessionAsync(Event eventItem)
    {
        var session = await _db.TalentShowSessionStates
            .FirstOrDefaultAsync(s => s.EventId == eventItem.Id);
        if (session != null)
        {
            return session;
        }

        session = new TalentShowSessionState
        {
            EventId = eventItem.Id,
            SessionCode = BuildSessionCode(eventItem),
            ShowName = string.IsNullOrWhiteSpace(eventItem.Title) ? "Talent Show" : eventItem.Title.Trim(),
            CurrentState = TalentShowLifecycleState.PreShow,
            OverallState = TalentShowOverallState.PreShow,
            LiveSubState = TalentShowLiveSubState.HostTalk,
            NextLiveSubState = TalentShowLiveSubState.ActShow,
            CurrentIndex = -1,
            SegmentStartedAtUtc = DateTime.UtcNow,
            EstimatedSegmentMinutes = 5,
            LastUpdated = DateTime.UtcNow
        };

        _db.TalentShowSessionStates.Add(session);
        await _db.SaveChangesAsync();
        return session;
    }

    private static TalentShowRealtimeState ToRealtimeState(
        TalentShowSessionState session,
        IReadOnlyList<TalentShowAct> acts,
        IReadOnlyList<TalentShowVote> votes)
    {
        var presentation = ParsePresentation(session.PresentationJson);

        var state = new TalentShowRealtimeState
        {
            ShowName = session.ShowName,
            SessionCode = session.SessionCode,
            CurrentState = session.CurrentState,
            IsLiveMode = session.IsLiveMode,
            OverallState = session.OverallState,
            LiveSubState = session.LiveSubState,
            NextLiveSubState = session.NextLiveSubState,
            SegmentStartedAtUtc = session.SegmentStartedAtUtc,
            EstimatedSegmentMinutes = session.EstimatedSegmentMinutes,
            CurrentIndex = session.CurrentIndex,
            UpdatedAtUtc = session.LastUpdated,
            Presentation = presentation,
            Items = acts
                .OrderBy(a => a.OrderIndex)
                .Select(a => new TalentShowScheduleItem
                {
                    Order = a.OrderIndex,
                    PerformerName = a.PerformerName,
                    ActTitle = a.Title,
                    Category = a.Category,
                    DurationMinutes = Math.Max(1, (int)Math.Round(a.DurationSeconds / 60.0)),
                    IntroLine = a.IntroLine,
                    OutroLine = a.OutroLine
                })
                .ToList(),
            Votes = votes
                .OrderBy(v => v.TimestampUtc)
                .Select(v => new TalentShowVoteSubmission
                {
                    VoterName = v.VoterName,
                    IsJudge = v.IsJudge,
                    ActOrder = acts.FirstOrDefault(a => a.Id == v.ActId)?.OrderIndex ?? 0,
                    TalentScore = v.TalentScore,
                    StagePresenceScore = v.StagePresenceScore,
                    CreativityScore = v.CreativityScore,
                    CrowdEngagementScore = v.CrowdEngagementScore,
                    SubmittedAtUtc = v.TimestampUtc
                })
                .ToList()
        };

        if (state.CurrentIndex < -1 || state.CurrentIndex >= state.Items.Count)
        {
            state.CurrentIndex = state.Items.Any() ? 0 : -1;
        }

        return state;
    }

    private static void ApplyRealtimeStateToSession(
        TalentShowSessionState session,
        TalentShowRealtimeState incomingState,
        IReadOnlyList<TalentShowAct> acts)
    {
        session.ShowName = string.IsNullOrWhiteSpace(incomingState.ShowName) ? session.ShowName : incomingState.ShowName.Trim();
        session.SessionCode = NormalizeSessionCode(string.IsNullOrWhiteSpace(incomingState.SessionCode) ? session.SessionCode : incomingState.SessionCode);
        session.CurrentState = NormalizeCurrentState(incomingState.CurrentState, incomingState.OverallState, incomingState.LiveSubState);
        session.OverallState = NormalizeOverallState(incomingState.OverallState, session.CurrentState);
        session.LiveSubState = NormalizeLiveSubState(incomingState.LiveSubState, session.CurrentState);
        session.NextLiveSubState = NormalizeLiveSubState(incomingState.NextLiveSubState, TalentShowLifecycleState.ActShow);
        session.IsLiveMode = session.OverallState == TalentShowOverallState.Live;
        session.CurrentIndex = incomingState.CurrentIndex;
        session.SegmentStartedAtUtc = incomingState.SegmentStartedAtUtc;
        session.EstimatedSegmentMinutes = incomingState.EstimatedSegmentMinutes <= 0 ? 5 : incomingState.EstimatedSegmentMinutes;
        session.PresentationJson = JsonSerializer.Serialize(incomingState.Presentation ?? new TalentShowPresentationSettings());

        var orderedActs = acts.OrderBy(a => a.OrderIndex).ToList();
        if (session.CurrentIndex >= 0 && session.CurrentIndex < orderedActs.Count)
        {
            session.CurrentActId = orderedActs[session.CurrentIndex].Id;
            session.NextActId = session.CurrentIndex + 1 < orderedActs.Count ? orderedActs[session.CurrentIndex + 1].Id : null;
        }
        else
        {
            session.CurrentActId = null;
            session.NextActId = orderedActs.FirstOrDefault()?.Id;
        }
    }

    private async Task SeedActsIfEmptyAsync(int eventId, IReadOnlyList<TalentShowAct> existingActs, IReadOnlyList<TalentShowScheduleItem> incomingItems)
    {
        if (existingActs.Any() || !incomingItems.Any())
        {
            return;
        }

        var normalizedItems = incomingItems
            .OrderBy(i => i.Order)
            .Select((item, index) => new TalentShowAct
            {
                EventId = eventId,
                PerformerName = item.PerformerName?.Trim() ?? string.Empty,
                Title = item.ActTitle?.Trim() ?? string.Empty,
                Category = item.Category?.Trim() ?? string.Empty,
                DurationSeconds = Math.Max(30, item.DurationMinutes * 60),
                OrderIndex = index + 1,
                IntroLine = item.IntroLine?.Trim() ?? string.Empty,
                OutroLine = item.OutroLine?.Trim() ?? string.Empty,
                SelectedForShow = true
            })
            .ToList();

        if (!normalizedItems.Any())
        {
            return;
        }

        _db.TalentShowActs.AddRange(normalizedItems);
        await _db.SaveChangesAsync();
    }

    private static TalentShowPresentationSettings ParsePresentation(string? presentationJson)
    {
        if (string.IsNullOrWhiteSpace(presentationJson))
        {
            return new TalentShowPresentationSettings();
        }

        try
        {
            return JsonSerializer.Deserialize<TalentShowPresentationSettings>(presentationJson, JsonOptions)
                ?? new TalentShowPresentationSettings();
        }
        catch (JsonException)
        {
            return new TalentShowPresentationSettings();
        }
    }

    private static string BuildSessionCode(Event eventItem)
    {
        if (string.IsNullOrWhiteSpace(eventItem.Slug))
        {
            return $"TALENT{eventItem.Id}";
        }

        var compact = new string(eventItem.Slug.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        if (compact.Length > 12)
        {
            compact = compact[..12];
        }

        return string.IsNullOrWhiteSpace(compact)
            ? $"TALENT{eventItem.Id}"
            : $"{compact}{eventItem.Id}";
    }

    private static string NormalizeSessionCode(string code)
    {
        return new string(code.Trim().ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
    }

    private static string NormalizeCurrentState(string? requestedCurrentState, string? requestedOverallState, string? requestedLiveSubState)
    {
        if (!string.IsNullOrWhiteSpace(requestedCurrentState) &&
            TalentShowLifecycleState.All.Contains(requestedCurrentState, StringComparer.OrdinalIgnoreCase))
        {
            return TalentShowLifecycleState.All.First(s => s.Equals(requestedCurrentState, StringComparison.OrdinalIgnoreCase));
        }

        if (string.Equals(requestedOverallState, TalentShowOverallState.PostShow, StringComparison.OrdinalIgnoreCase))
        {
            return TalentShowLifecycleState.PostShow;
        }

        if (string.Equals(requestedOverallState, TalentShowOverallState.Live, StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(requestedLiveSubState, TalentShowLiveSubState.HostTalk, StringComparison.OrdinalIgnoreCase))
            {
                return TalentShowLifecycleState.HostTalk;
            }

            if (string.Equals(requestedLiveSubState, TalentShowLiveSubState.PauseIntermission, StringComparison.OrdinalIgnoreCase))
            {
                return TalentShowLifecycleState.Intermission;
            }

            return TalentShowLifecycleState.ActShow;
        }

        return TalentShowLifecycleState.PreShow;
    }

    private static string NormalizeOverallState(string? requestedOverallState, string currentState)
    {
        if (!string.IsNullOrWhiteSpace(requestedOverallState) &&
            TalentShowOverallState.All.Contains(requestedOverallState, StringComparer.OrdinalIgnoreCase))
        {
            return TalentShowOverallState.All.First(s => s.Equals(requestedOverallState, StringComparison.OrdinalIgnoreCase));
        }

        if (currentState == TalentShowLifecycleState.PostShow)
        {
            return TalentShowOverallState.PostShow;
        }

        if (currentState is TalentShowLifecycleState.HostTalk or TalentShowLifecycleState.ActShow or TalentShowLifecycleState.Intermission)
        {
            return TalentShowOverallState.Live;
        }

        return TalentShowOverallState.PreShow;
    }

    private static string NormalizeLiveSubState(string? requestedLiveSubState, string currentState)
    {
        if (!string.IsNullOrWhiteSpace(requestedLiveSubState) &&
            TalentShowLiveSubState.All.Contains(requestedLiveSubState, StringComparer.OrdinalIgnoreCase))
        {
            return TalentShowLiveSubState.All.First(s => s.Equals(requestedLiveSubState, StringComparison.OrdinalIgnoreCase));
        }

        return currentState switch
        {
            TalentShowLifecycleState.HostTalk => TalentShowLiveSubState.HostTalk,
            TalentShowLifecycleState.Intermission => TalentShowLiveSubState.PauseIntermission,
            _ => TalentShowLiveSubState.ActShow
        };
    }

    private async Task<int> NextOrderIndexAsync(int eventId)
    {
        var maxOrder = await _db.TalentShowActs
            .Where(a => a.EventId == eventId)
            .Select(a => (int?)a.OrderIndex)
            .MaxAsync();
        return (maxOrder ?? 0) + 1;
    }

    private static int ClampScore(int score) => Math.Clamp(score, 1, 5);

    private async Task<EventControl?> GetTalentShowDirectorControlAsync(int eventId)
    {
        return await _db.EventControls
            .FirstOrDefaultAsync(c => c.EventId == eventId && c.ControlType == TalentShowDirectorControlType && c.IsDirector);
    }

    private async Task<TalentShowSignupSettingsDTO> GetNormalizedSignupSettingsAsync(Event eventItem)
    {
        var control = await GetTalentShowDirectorControlAsync(eventItem.Id);
        var settings = control == null
            ? new TalentShowDirectorSettings()
            : ParseDirectorSettings(control.SettingsJson);

        var planningForDefaults = NormalizePlanningConfig(ApplyGlobalDefaultsToPlanning(
            NormalizePlanningConfig(settings.Planning),
            NormalizeGlobalSetupConfig(settings.GlobalSetup, eventItem)));

        return NormalizeSignupSettings(settings.Signups, planningForDefaults, eventItem);
    }

    private static bool IsSignupWindowOpen(TalentShowSignupSettingsDTO settings)
    {
        var now = DateTime.UtcNow;
        var startUtc = settings.SignupStart?.ToUniversalTime();
        var endUtc = settings.SignupEnd?.ToUniversalTime();

        if (startUtc.HasValue && now < startUtc.Value)
        {
            return false;
        }

        if (endUtc.HasValue && now > endUtc.Value)
        {
            return false;
        }

        return true;
    }

    private async Task<EventControl> GetOrCreateTalentShowDirectorControlAsync(Event eventItem)
    {
        var existingControl = await GetTalentShowDirectorControlAsync(eventItem.Id);
        if (existingControl != null)
        {
            return existingControl;
        }

        var nextOrder = await _db.EventControls
            .Where(c => c.EventId == eventItem.Id)
            .Select(c => (int?)c.SequenceOrder)
            .MaxAsync() ?? 0;

        var newControl = new EventControl
        {
            EventId = eventItem.Id,
            ControlType = TalentShowDirectorControlType,
            DisplayName = TalentShowDirectorControlType,
            IsDirector = true,
            PublicInformation = false,
            SettingsJson = "{}",
            NotesMarkdown = string.Empty,
            NotesHtml = string.Empty,
            SequenceOrder = nextOrder + 1,
            IsActive = true
        };

        _db.EventControls.Add(newControl);
        return newControl;
    }

    private static TalentShowPlanningConfigDTO NormalizePlanningConfig(TalentShowPlanningConfigDTO dto)
    {
        var normalized = dto ?? new TalentShowPlanningConfigDTO();

        normalized.MeetingDates = (normalized.MeetingDates ?? [])
            .Where(meeting => meeting.Date != default)
            .OrderBy(meeting => meeting.Date)
            .Select(meeting => new TalentShowPlanningMeetingDateDTO
            {
                MeetingDateId = string.IsNullOrWhiteSpace(meeting.MeetingDateId)
                    ? Guid.NewGuid().ToString("N")
                    : meeting.MeetingDateId.Trim(),
                Date = meeting.Date,
                AddedByUserId = meeting.AddedByUserId?.Trim() ?? string.Empty,
                LockedToUserId = string.IsNullOrWhiteSpace(meeting.LockedToUserId)
                    ? null
                    : meeting.LockedToUserId.Trim(),
                Notes = meeting.Notes?.Trim() ?? string.Empty
            })
            .ToList();

        normalized.AgendaTemplateItems = (normalized.AgendaTemplateItems ?? [])
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        normalized.AssignedHelpers = (normalized.AssignedHelpers ?? [])
            .Where(helper => !string.IsNullOrWhiteSpace(helper))
            .Select(helper => helper.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        normalized.MaxActs = normalized.MaxActs <= 0 ? 25 : normalized.MaxActs;
        normalized.JudgeCount = normalized.JudgeCount <= 0 ? 1 : normalized.JudgeCount;
        normalized.ExternalSignupUrl = string.IsNullOrWhiteSpace(normalized.ExternalSignupUrl)
            ? null
            : normalized.ExternalSignupUrl.Trim();
        normalized.RulesMarkdown = normalized.RulesMarkdown?.Trim() ?? string.Empty;
        normalized.RulesPdfPath = string.IsNullOrWhiteSpace(normalized.RulesPdfPath)
            ? null
            : normalized.RulesPdfPath.Trim();
        normalized.JudgeNames = (normalized.JudgeNames ?? [])
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.SignupStart.HasValue && normalized.SignupEnd.HasValue && normalized.SignupEnd < normalized.SignupStart)
        {
            (normalized.SignupStart, normalized.SignupEnd) = (normalized.SignupEnd, normalized.SignupStart);
        }

        if (normalized.MaxActsFlexible)
        {
            var min = normalized.MaxActsMin ?? normalized.MaxActs;
            var max = normalized.MaxActsMax ?? normalized.MaxActs;
            min = min <= 0 ? 1 : min;
            max = max < min ? min : max;

            normalized.MaxActsMin = min;
            normalized.MaxActsMax = max;
            normalized.MaxActs = Math.Clamp(normalized.MaxActs, min, max);
        }
        else
        {
            normalized.MaxActsMin = null;
            normalized.MaxActsMax = null;
        }

        normalized.Categories = (normalized.Categories ?? [])
            .Where(category => !string.IsNullOrWhiteSpace(category.Name))
            .Select((category, index) => new TalentShowPlanningCategoryDTO
            {
                CategoryId = string.IsNullOrWhiteSpace(category.CategoryId)
                    ? $"cat-{index + 1}"
                    : category.CategoryId.Trim(),
                Name = category.Name.Trim(),
                Description = category.Description?.Trim() ?? string.Empty,
                OrderIndex = index + 1
            })
            .ToList();

        normalized.Notes = (normalized.Notes ?? [])
            .Where(note => !string.IsNullOrWhiteSpace(note.Text))
            .Select(note => new TalentShowPlanningNoteDTO
            {
                NoteId = string.IsNullOrWhiteSpace(note.NoteId) ? Guid.NewGuid().ToString("N") : note.NoteId.Trim(),
                Text = note.Text.Trim(),
                CreatedAt = note.CreatedAt == default ? DateTime.UtcNow : note.CreatedAt,
                CreatedBy = note.CreatedBy?.Trim() ?? string.Empty
            })
            .ToList();

        normalized.Questions = (normalized.Questions ?? [])
            .Where(question => !string.IsNullOrWhiteSpace(question.Text))
            .Select(question => new TalentShowPlanningQuestionDTO
            {
                QuestionId = string.IsNullOrWhiteSpace(question.QuestionId) ? Guid.NewGuid().ToString("N") : question.QuestionId.Trim(),
                Text = question.Text.Trim(),
                Resolved = question.Resolved,
                ResolvedAt = question.Resolved ? question.ResolvedAt ?? DateTime.UtcNow : null
            })
            .ToList();

        return normalized;
    }

    private static TalentShowSignupSettingsDTO NormalizeSignupSettings(
        TalentShowSignupSettingsDTO? dto,
        TalentShowPlanningConfigDTO planning,
        Event eventItem)
    {
        var normalized = dto ?? new TalentShowSignupSettingsDTO();
        normalized.Title = string.IsNullOrWhiteSpace(normalized.Title)
            ? "Talent Show Sign-Ups"
            : normalized.Title.Trim();
        normalized.Subtitle = normalized.Subtitle?.Trim() ?? string.Empty;
        normalized.PublicUrl = string.IsNullOrWhiteSpace(normalized.PublicUrl)
            ? $"/talent-show/signups/{eventItem.Id}"
            : normalized.PublicUrl.Trim();
        normalized.ConfirmationMessage = normalized.ConfirmationMessage?.Trim() ?? string.Empty;

        if (normalized.SignupStart == null && planning.SignupStart != null)
        {
            normalized.SignupStart = planning.SignupStart;
        }

        if (normalized.SignupEnd == null && planning.SignupEnd != null)
        {
            normalized.SignupEnd = planning.SignupEnd;
        }

        if (normalized.SignupStart.HasValue && normalized.SignupEnd.HasValue && normalized.SignupEnd < normalized.SignupStart)
        {
            (normalized.SignupStart, normalized.SignupEnd) = (normalized.SignupEnd, normalized.SignupStart);
        }

        return normalized;
    }

    private static TalentShowTryOutsConfigDTO NormalizeTryOutsConfig(TalentShowTryOutsConfigDTO? dto)
    {
        var normalized = dto ?? new TalentShowTryOutsConfigDTO();
        normalized.SlotLengthMinutes = normalized.SlotLengthMinutes <= 0 ? 5 : normalized.SlotLengthMinutes;
        normalized.ScoringScaleMax = normalized.ScoringScaleMax <= 0 ? 5 : normalized.ScoringScaleMax;
        normalized.ScoringFields = (normalized.ScoringFields ?? [])
            .Where(field => !string.IsNullOrWhiteSpace(field.Label))
            .Select((field, index) => new TalentShowTryOutScoringFieldDTO
            {
                FieldId = string.IsNullOrWhiteSpace(field.FieldId)
                    ? $"score-{index + 1}"
                    : field.FieldId.Trim(),
                Label = field.Label.Trim(),
                MaxScore = field.MaxScore <= 0 ? normalized.ScoringScaleMax : field.MaxScore,
                OrderIndex = index + 1
            })
            .ToList();

        if (!normalized.ScoringFields.Any())
        {
            normalized.ScoringFields =
            [
                new TalentShowTryOutScoringFieldDTO { FieldId = "score-1", Label = "Stage presence", MaxScore = normalized.ScoringScaleMax, OrderIndex = 1 },
                new TalentShowTryOutScoringFieldDTO { FieldId = "score-2", Label = "Preparedness", MaxScore = normalized.ScoringScaleMax, OrderIndex = 2 },
                new TalentShowTryOutScoringFieldDTO { FieldId = "score-3", Label = "Creativity", MaxScore = normalized.ScoringScaleMax, OrderIndex = 3 },
                new TalentShowTryOutScoringFieldDTO { FieldId = "score-4", Label = "Overall impression", MaxScore = normalized.ScoringScaleMax, OrderIndex = 4 }
            ];
        }

        normalized.Sessions = (normalized.Sessions ?? [])
            .Where(session => !string.IsNullOrWhiteSpace(session.SessionId))
            .Select(session => new TalentShowTryOutSessionDTO
            {
                SessionId = session.SessionId.Trim(),
                Date = session.Date,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Location = session.Location?.Trim() ?? string.Empty,
                PublicVisible = session.PublicVisible,
                ParentSignOutRequired = session.ParentSignOutRequired
            })
            .GroupBy(session => session.SessionId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();

        normalized.ActiveSessionId = string.IsNullOrWhiteSpace(normalized.ActiveSessionId)
            ? string.Empty
            : normalized.ActiveSessionId.Trim();
        if (!string.IsNullOrWhiteSpace(normalized.ActiveSessionId) &&
            !normalized.Sessions.Any(session =>
                session.SessionId.Equals(normalized.ActiveSessionId, StringComparison.OrdinalIgnoreCase)))
        {
            normalized.ActiveSessionId = string.Empty;
        }

        return normalized;
    }

    private static TalentShowGlobalSetupConfigDTO NormalizeGlobalSetupConfig(TalentShowGlobalSetupConfigDTO? dto, Event? eventItem = null)
    {
        var normalized = dto ?? new TalentShowGlobalSetupConfigDTO();
        normalized.EventTitle = string.IsNullOrWhiteSpace(normalized.EventTitle)
            ? (eventItem?.Title?.Trim() ?? "Talent Show")
            : normalized.EventTitle.Trim();
        normalized.Theme = normalized.Theme?.Trim() ?? string.Empty;
        normalized.BrandingMessage = normalized.BrandingMessage?.Trim() ?? string.Empty;
        normalized.DefaultDisplaySettings = normalized.DefaultDisplaySettings?.Trim() ?? string.Empty;
        normalized.DefaultMediaRules = normalized.DefaultMediaRules?.Trim() ?? string.Empty;
        normalized.DefaultCueRules = normalized.DefaultCueRules?.Trim() ?? string.Empty;
        normalized.DefaultSegmentBehavior = normalized.DefaultSegmentBehavior?.Trim() ?? string.Empty;
        normalized.DefaultMaxActs = normalized.DefaultMaxActs <= 0 ? 25 : normalized.DefaultMaxActs;
        normalized.DefaultJudgeCount = normalized.DefaultJudgeCount <= 0 ? 3 : normalized.DefaultJudgeCount;
        normalized.DefaultExternalSignupUrl = string.IsNullOrWhiteSpace(normalized.DefaultExternalSignupUrl)
            ? null
            : normalized.DefaultExternalSignupUrl.Trim();
        normalized.DefaultRulesMarkdown = normalized.DefaultRulesMarkdown?.Trim() ?? string.Empty;
        normalized.DefaultRulesPdfPath = string.IsNullOrWhiteSpace(normalized.DefaultRulesPdfPath)
            ? null
            : normalized.DefaultRulesPdfPath.Trim();
        normalized.DefaultJudgeNames = (normalized.DefaultJudgeNames ?? [])
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        normalized.DefaultCategories = (normalized.DefaultCategories ?? [])
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Select(category => category.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.DefaultSignupStart.HasValue && normalized.DefaultSignupEnd.HasValue &&
            normalized.DefaultSignupEnd < normalized.DefaultSignupStart)
        {
            (normalized.DefaultSignupStart, normalized.DefaultSignupEnd) = (normalized.DefaultSignupEnd, normalized.DefaultSignupStart);
        }

        if (normalized.DefaultMaxActsFlexible)
        {
            var min = normalized.DefaultMaxActsMin ?? normalized.DefaultMaxActs;
            var max = normalized.DefaultMaxActsMax ?? normalized.DefaultMaxActs;
            min = min <= 0 ? 1 : min;
            max = max < min ? min : max;
            normalized.DefaultMaxActsMin = min;
            normalized.DefaultMaxActsMax = max;
            normalized.DefaultMaxActs = Math.Clamp(normalized.DefaultMaxActs, min, max);
        }
        else
        {
            normalized.DefaultMaxActsMin = null;
            normalized.DefaultMaxActsMax = null;
        }

        return normalized;
    }

    private static TalentShowPlanningConfigDTO ApplyGlobalDefaultsToPlanning(
        TalentShowPlanningConfigDTO planning,
        TalentShowGlobalSetupConfigDTO globalSetup)
    {
        var merged = planning ?? new TalentShowPlanningConfigDTO();
        var global = globalSetup ?? new TalentShowGlobalSetupConfigDTO();

        if (merged.MaxActs == 25 && global.DefaultMaxActs > 0)
        {
            merged.MaxActs = global.DefaultMaxActs;
        }

        if (!merged.MaxActsFlexible && global.DefaultMaxActsFlexible)
        {
            merged.MaxActsFlexible = true;
        }

        if (merged.MaxActsMin == null && global.DefaultMaxActsMin.HasValue)
        {
            merged.MaxActsMin = global.DefaultMaxActsMin;
        }

        if (merged.MaxActsMax == null && global.DefaultMaxActsMax.HasValue)
        {
            merged.MaxActsMax = global.DefaultMaxActsMax;
        }

        if (merged.JudgeCount == 3 && global.DefaultJudgeCount > 0)
        {
            merged.JudgeCount = global.DefaultJudgeCount;
        }

        if (!merged.JudgeNames.Any() && global.DefaultJudgeNames.Any())
        {
            merged.JudgeNames = global.DefaultJudgeNames.ToList();
        }

        if (merged.SignupStart == null && global.DefaultSignupStart != null)
        {
            merged.SignupStart = global.DefaultSignupStart;
        }

        if (merged.SignupEnd == null && global.DefaultSignupEnd != null)
        {
            merged.SignupEnd = global.DefaultSignupEnd;
        }

        if (string.IsNullOrWhiteSpace(merged.ExternalSignupUrl) && !string.IsNullOrWhiteSpace(global.DefaultExternalSignupUrl))
        {
            merged.ExternalSignupUrl = global.DefaultExternalSignupUrl;
        }

        if (!merged.Categories.Any() && global.DefaultCategories.Any())
        {
            merged.Categories = global.DefaultCategories
                .Select((name, index) => new TalentShowPlanningCategoryDTO
                {
                    CategoryId = $"cat-{index + 1}",
                    Name = name,
                    OrderIndex = index + 1
                })
                .ToList();
        }

        if (string.IsNullOrWhiteSpace(merged.RulesMarkdown) && !string.IsNullOrWhiteSpace(global.DefaultRulesMarkdown))
        {
            merged.RulesMarkdown = global.DefaultRulesMarkdown;
        }

        if (string.IsNullOrWhiteSpace(merged.RulesPdfPath) && !string.IsNullOrWhiteSpace(global.DefaultRulesPdfPath))
        {
            merged.RulesPdfPath = global.DefaultRulesPdfPath;
        }

        if (!merged.PublishRules && global.DefaultPublishRules &&
            (string.IsNullOrWhiteSpace(merged.RulesMarkdown) || string.IsNullOrWhiteSpace(merged.RulesPdfPath)))
        {
            merged.PublishRules = true;
        }

        return merged;
    }

    private static string NormalizeTryOutEntryStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return TalentShowTryOutEntryStatus.Invited;
        }

        var normalized = status.Trim();
        if (normalized.Equals("Scheduled", StringComparison.OrdinalIgnoreCase) ||
            normalized.Equals("Arrived", StringComparison.OrdinalIgnoreCase))
        {
            return TalentShowTryOutEntryStatus.StandBy;
        }

        if (normalized.Equals("NoShow", StringComparison.OrdinalIgnoreCase))
        {
            return TalentShowTryOutEntryStatus.Rejected;
        }

        return TalentShowTryOutEntryStatus.All.FirstOrDefault(s =>
                    s.Equals(normalized, StringComparison.OrdinalIgnoreCase))
               ?? TalentShowTryOutEntryStatus.Invited;
    }

    private static string NormalizeSignupAction(string? action)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            return string.Empty;
        }

        var normalized = action.Trim();
        return TalentShowSignupStatus.All.FirstOrDefault(status =>
            status.Equals(normalized, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
    }

    private async Task EnsureActForSignupAsync(int eventId, TalentShowSignup signup)
    {
        var existingAct = await _db.TalentShowActs.FirstOrDefaultAsync(a =>
            a.EventId == eventId &&
            a.PerformerName == signup.PerformerNames &&
            a.Title == signup.ActTitle);

        if (existingAct != null)
        {
            existingAct.SelectedForShow = true;
            if (!string.IsNullOrWhiteSpace(signup.MusicUrl))
            {
                existingAct.MediaFilePath = signup.MusicUrl;
            }
            if (string.IsNullOrWhiteSpace(existingAct.Notes))
            {
                existingAct.Notes = signup.SpecialRequirements;
            }

            return;
        }

        var category = await TryResolveCategoryForSignupAsync(eventId);
        var newAct = new TalentShowAct
        {
            EventId = eventId,
            PerformerName = signup.PerformerNames,
            Title = signup.ActTitle,
            Category = category,
            DurationSeconds = 60,
            OrderIndex = await NextOrderIndexAsync(eventId),
            IntroLine = string.Empty,
            OutroLine = string.Empty,
            MediaFilePath = string.IsNullOrWhiteSpace(signup.MusicUrl) ? signup.MediaUpload : signup.MusicUrl,
            Notes = signup.SpecialRequirements,
            SelectedForShow = true
        };

        _db.TalentShowActs.Add(newAct);
    }

    private async Task EnsureTryOutEntryForSignupAsync(int eventId, TalentShowSignup signup, string? sessionLabel)
    {
        var existingEntry = await _db.TalentShowTryOutEntries.FirstOrDefaultAsync(te =>
            te.EventId == eventId && te.SignupId == signup.Id);

        if (existingEntry != null)
        {
            existingEntry.SessionLabel = string.IsNullOrWhiteSpace(sessionLabel)
                ? existingEntry.SessionLabel
                : sessionLabel.Trim();
            existingEntry.Status = TalentShowTryOutEntryStatus.Invited;
            existingEntry.PickupAdult = signup.PickupAdult;
            existingEntry.MusicUrl = signup.MusicUrl;
            existingEntry.UpdatedAtUtc = DateTime.UtcNow;
            return;
        }

        var newEntry = new TalentShowTryOutEntry
        {
            EventId = eventId,
            SignupId = signup.Id,
            PerformerNames = signup.PerformerNames,
            ActTitle = signup.ActTitle,
            MusicUrl = signup.MusicUrl,
            PickupAdult = signup.PickupAdult,
            SessionLabel = sessionLabel?.Trim() ?? string.Empty,
            Notes = signup.SpecialRequirements,
            Selected = false,
            Status = TalentShowTryOutEntryStatus.Invited,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _db.TalentShowTryOutEntries.Add(newEntry);
    }

    private async Task EnsureActForTryOutEntryAsync(int eventId, TalentShowTryOutEntry entry)
    {
        var existingAct = await _db.TalentShowActs.FirstOrDefaultAsync(a =>
            a.EventId == eventId &&
            a.PerformerName == entry.PerformerNames &&
            a.Title == entry.ActTitle);

        if (existingAct != null)
        {
            existingAct.SelectedForShow = true;
            if (string.IsNullOrWhiteSpace(existingAct.Notes))
            {
                existingAct.Notes = entry.Notes;
            }
            if (!string.IsNullOrWhiteSpace(entry.MusicUrl))
            {
                existingAct.MediaFilePath = entry.MusicUrl;
            }

            entry.ActId = existingAct.Id;
            return;
        }

        var category = await TryResolveCategoryForSignupAsync(eventId);
        var act = new TalentShowAct
        {
            EventId = eventId,
            PerformerName = entry.PerformerNames,
            Title = entry.ActTitle,
            Category = category,
            DurationSeconds = 60,
            OrderIndex = await NextOrderIndexAsync(eventId),
            IntroLine = string.Empty,
            OutroLine = string.Empty,
            MediaFilePath = entry.MusicUrl,
            Notes = BuildTryOutNotes(entry),
            SelectedForShow = true
        };

        _db.TalentShowActs.Add(act);
        await _db.SaveChangesAsync();
        entry.ActId = act.Id;
    }

    private async Task<string> TryResolveCategoryForSignupAsync(int eventId)
    {
        var control = await GetTalentShowDirectorControlAsync(eventId);
        if (control == null)
        {
            return string.Empty;
        }

        var settings = ParseDirectorSettings(control.SettingsJson);
        return settings.Planning.Categories
            .OrderBy(c => c.OrderIndex)
            .Select(c => c.Name)
            .FirstOrDefault() ?? string.Empty;
    }

    private async Task EnsureSegmentStubForTryOutEntryAsync(int eventId, TalentShowTryOutEntry entry)
    {
        if (entry.ActId == null)
        {
            return;
        }

        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return;
        }

        var control = await GetOrCreateTalentShowDirectorControlAsync(eventItem);
        var settings = ParseDirectorSettings(control.SettingsJson);
        settings.Show ??= new TalentShowShowConfigDTO();
        settings.Show.Segments ??= [];

        var existingSegment = settings.Show.Segments.FirstOrDefault(segment =>
            segment.ActId == entry.ActId &&
            segment.SegmentType == TalentShowSegmentType.Act);

        var act = await _db.TalentShowActs.FirstOrDefaultAsync(a => a.Id == entry.ActId.Value && a.EventId == eventId);
        if (act == null)
        {
            return;
        }

        var title = string.IsNullOrWhiteSpace(entry.ActTitle) ? act.Title : entry.ActTitle;
        var mainBoardText = $"{entry.PerformerNames} - {title}".Trim(' ', '-');
        var scoringSummary = BuildScoringSummary(entry.ScoresJson);
        var backstageNotes = BuildTryOutNotes(entry, scoringSummary);

        if (existingSegment == null)
        {
            var nextSegmentId = settings.Show.Segments.Select(s => s.SegmentId).DefaultIfEmpty(0).Max() + 1;
            settings.Show.Segments.Add(new TalentShowSegmentDTO
            {
                SegmentId = nextSegmentId,
                SegmentType = TalentShowSegmentType.Act,
                ActId = act.Id,
                Title = title,
                OrderIndex = settings.Show.Segments.Count + 1,
                IsHidden = false,
                DisplayInstructions = new TalentShowDisplayInstructionsDTO
                {
                    MainBoard = mainBoardText,
                    Backstage = backstageNotes,
                    HostTeleprompter = string.Empty,
                    TimerPresetSeconds = act.DurationSeconds <= 0 ? 60 : act.DurationSeconds
                },
                MediaFile = string.IsNullOrWhiteSpace(entry.MusicUrl) ? act.MediaFilePath : entry.MusicUrl,
                CueList = [],
                TechRequirements = string.Empty,
                ExpectedDurationSeconds = act.DurationSeconds <= 0 ? 60 : act.DurationSeconds,
                ActualDurationSeconds = 0,
                RunsLong = false,
                Status = TalentShowSegmentStatus.Waiting
            });
        }
        else
        {
            existingSegment.Title = title;
            existingSegment.DisplayInstructions ??= new TalentShowDisplayInstructionsDTO();
            existingSegment.DisplayInstructions.MainBoard = mainBoardText;
            existingSegment.DisplayInstructions.Backstage = backstageNotes;
            existingSegment.DisplayInstructions.HostTeleprompter = string.Empty;
            existingSegment.DisplayInstructions.TimerPresetSeconds = act.DurationSeconds <= 0 ? 60 : act.DurationSeconds;
            existingSegment.MediaFile = string.IsNullOrWhiteSpace(entry.MusicUrl) ? act.MediaFilePath : entry.MusicUrl;
            existingSegment.CueList = [];
            existingSegment.TechRequirements = string.Empty;
            existingSegment.ExpectedDurationSeconds = act.DurationSeconds <= 0 ? 60 : act.DurationSeconds;
        }

        control.SettingsJson = JsonSerializer.Serialize(settings, JsonOptions);
    }

    private static string BuildTryOutNotes(TalentShowTryOutEntry entry, string? scoringSummary = null)
    {
        var summary = string.IsNullOrWhiteSpace(scoringSummary)
            ? BuildScoringSummary(entry.ScoresJson)
            : scoringSummary;

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(entry.Notes))
        {
            parts.Add($"TryOutNotes: {entry.Notes.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(summary))
        {
            parts.Add($"ScoringSummary: {summary}");
        }

        return string.Join(" | ", parts);
    }

    private static string BuildScoringSummary(string? scoresJson)
    {
        if (string.IsNullOrWhiteSpace(scoresJson))
        {
            return string.Empty;
        }

        try
        {
            var scores = JsonSerializer.Deserialize<List<TalentShowTryOutScoreDTO>>(scoresJson, JsonOptions) ?? [];
            if (!scores.Any())
            {
                return string.Empty;
            }

            return string.Join(", ", scores
                .Where(score => !string.IsNullOrWhiteSpace(score.Category))
                .Select(score => $"{score.Category.Trim()}: {score.Value}"));
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private async Task<List<TalentShowTryOutEntry>> GetStandByEntriesForPdfAsync(int eventId, string? sessionId)
    {
        var entriesQuery = _db.TalentShowTryOutEntries
            .Where(entry => entry.EventId == eventId &&
                            entry.Status == TalentShowTryOutEntryStatus.StandBy &&
                            entry.CheckInTimestamp != null);

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            entriesQuery = entriesQuery.Where(entry => entry.SessionLabel == sessionId.Trim());
        }

        return await entriesQuery
            .OrderBy(entry => entry.CheckInTimestamp ?? entry.CreatedAtUtc)
            .ThenBy(entry => entry.PerformerNames)
            .ToListAsync();
    }

    private static string GetSessionLabel(string? sessionId)
    {
        return string.IsNullOrWhiteSpace(sessionId) ? "All Sessions" : sessionId.Trim();
    }

    private async Task ClearOtherOnDeckEntriesAsync(int eventId, string? sessionLabel, int? currentEntryId)
    {
        var normalizedSessionLabel = sessionLabel?.Trim() ?? string.Empty;
        var onDeckQuery = _db.TalentShowTryOutEntries.Where(entry =>
            entry.EventId == eventId &&
            entry.Status == TalentShowTryOutEntryStatus.OnDeck &&
            entry.SessionLabel == normalizedSessionLabel);

        if (currentEntryId.HasValue)
        {
            onDeckQuery = onDeckQuery.Where(entry => entry.Id != currentEntryId.Value);
        }

        var otherOnDeckEntries = await onDeckQuery.ToListAsync();
        if (!otherOnDeckEntries.Any())
        {
            return;
        }

        var nowUtc = DateTime.UtcNow;
        foreach (var otherEntry in otherOnDeckEntries)
        {
            otherEntry.Status = TalentShowTryOutEntryStatus.StandBy;
            otherEntry.CheckInTimestamp ??= nowUtc;
            otherEntry.UpdatedAtUtc = nowUtc;
        }
    }

    private static byte[] BuildSimplePdf(string title, string subtitle, IReadOnlyList<string> lines)
    {
        var escapedTitle = EscapePdfText(title);
        var escapedSubtitle = EscapePdfText(subtitle);
        var textLines = new List<string> { escapedTitle, escapedSubtitle, string.Empty };
        textLines.AddRange(lines.Select(EscapePdfText));

        var sb = new StringBuilder();
        sb.AppendLine("BT");
        sb.AppendLine("/F1 12 Tf");
        sb.AppendLine("72 770 Td");
        sb.AppendLine("14 TL");
        foreach (var line in textLines)
        {
            sb.AppendLine($"({line}) Tj");
            sb.AppendLine("T*");
        }
        sb.AppendLine("ET");
        var stream = sb.ToString();
        var streamBytes = Encoding.ASCII.GetBytes(stream);

        var objects = new List<string>
        {
            "1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj",
            "2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj",
            "3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >> endobj",
            $"4 0 obj << /Length {streamBytes.Length} >> stream\n{stream}endstream\nendobj",
            "5 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj"
        };

        var pdf = new StringBuilder();
        pdf.Append("%PDF-1.4\n");
        var offsets = new List<int> { 0 };
        foreach (var obj in objects)
        {
            offsets.Add(pdf.Length);
            pdf.Append(obj);
            pdf.Append('\n');
        }

        var xrefStart = pdf.Length;
        pdf.Append($"xref\n0 {offsets.Count}\n");
        pdf.Append("0000000000 65535 f \n");
        for (var i = 1; i < offsets.Count; i++)
        {
            pdf.Append($"{offsets[i]:D10} 00000 n \n");
        }

        pdf.Append($"trailer << /Size {offsets.Count} /Root 1 0 R >>\n");
        pdf.Append($"startxref\n{xrefStart}\n%%EOF");
        return Encoding.ASCII.GetBytes(pdf.ToString());
    }

    private static string EscapePdfText(string value)
    {
        return (value ?? string.Empty)
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
    }

    private async Task SyncTryOutSessionsToEventDaysAsync(int eventId, Event eventItem, List<TalentShowTryOutSessionDTO>? sessions)
    {
        // Get all existing try-outs-generated event days
        var existingTryOutDays = eventItem.EventDays
            .Where(d => d.IsFromTryOuts)
            .ToList();

        if (sessions == null || !sessions.Any())
        {
            // If no sessions, delete all try-outs-generated days
            foreach (var day in existingTryOutDays)
            {
                _db.EventDays.Remove(day);
            }
            return;
        }

        // Track which sessions have corresponding days
        var sessionsByDate = sessions.Where(s => s.Date.HasValue).ToList();
        var daysToDelete = new List<EventDay>();

        // Remove days that are no longer in sessions
        foreach (var existingDay in existingTryOutDays)
        {
            var hasSession = sessionsByDate.Any(s => s.Date.HasValue && s.Date.Value.ToDateTime(TimeOnly.MinValue) == existingDay.Date.Date);
            if (!hasSession)
            {
                daysToDelete.Add(existingDay);
            }
        }

        foreach (var dayToDelete in daysToDelete)
        {
            _db.EventDays.Remove(dayToDelete);
        }

        // Create or update days for each session with a date
        var dayNumber = eventItem.EventDays.Where(d => !d.IsFromTryOuts).Count() + 1;
        
        foreach (var session in sessionsByDate)
        {
            if (!session.Date.HasValue)
                continue;

            var sessionDate = session.Date.Value.ToDateTime(TimeOnly.MinValue);
            var existingDay = existingTryOutDays.FirstOrDefault(d => d.Date.Date == sessionDate.Date);
            
            if (existingDay != null)
            {
                // Update existing day
                existingDay.DayTitle = $"Try-Outs: {session.SessionId}";
                existingDay.Description = session.Location ?? string.Empty;
                existingDay.Location = session.Location ?? string.Empty;
                existingDay.StartTime = session.StartTime.HasValue
                    ? sessionDate.Date.Add(session.StartTime.Value.ToTimeSpan())
                    : null;
                existingDay.EndTime = session.EndTime.HasValue
                    ? sessionDate.Date.Add(session.EndTime.Value.ToTimeSpan())
                    : null;
                existingDay.PublicVisibleFromTryOuts = session.PublicVisible;
            }
            else
            {
                // Create new day
                var newDay = new EventDay
                {
                    EventId = eventId,
                    DayNumber = dayNumber++,
                    Date = sessionDate.Date,
                    DayTitle = $"Try-Outs: {session.SessionId}",
                    Description = session.Location ?? string.Empty,
                    Location = session.Location ?? string.Empty,
                    StartTime = session.StartTime.HasValue
                        ? sessionDate.Date.Add(session.StartTime.Value.ToTimeSpan())
                        : null,
                    EndTime = session.EndTime.HasValue
                        ? sessionDate.Date.Add(session.EndTime.Value.ToTimeSpan())
                        : null,
                    IsActive = true,
                    IsFromTryOuts = true,
                    PublicVisibleFromTryOuts = session.PublicVisible
                };
                _db.EventDays.Add(newDay);
            }
        }
    }

    private static TalentShowDirectorSettings ParseDirectorSettings(string? settingsJson)
    {
        if (string.IsNullOrWhiteSpace(settingsJson))
        {
            return new TalentShowDirectorSettings();
        }

        try
        {
            return JsonSerializer.Deserialize<TalentShowDirectorSettings>(settingsJson, JsonOptions)
                   ?? new TalentShowDirectorSettings();
        }
        catch (JsonException)
        {
            return new TalentShowDirectorSettings();
        }
    }

    private sealed class TalentShowDirectorSettings
    {
        public TalentShowPlanningConfigDTO Planning { get; set; } = new();
        public TalentShowGlobalSetupConfigDTO GlobalSetup { get; set; } = new();
        public TalentShowSignupSettingsDTO Signups { get; set; } = new();
        public TalentShowTryOutsConfigDTO TryOuts { get; set; } = new();
        public TalentShowShowConfigDTO Show { get; set; } = new();
    }

    // ============================================================================
    // THE SHOW MODULE ENDPOINTS
    // ============================================================================

    [HttpGet("show/configure")]
    public async Task<ActionResult<TalentShowShowConfigDTO>> GetShowConfiguration(int eventId)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var control = await GetTalentShowDirectorControlAsync(eventId);
        var settings = control == null
            ? new TalentShowDirectorSettings()
            : ParseDirectorSettings(control.SettingsJson);

        var acts = await _db.TalentShowActs
            .Where(a => a.EventId == eventId && a.SelectedForShow)
            .OrderBy(a => a.OrderIndex)
            .ToListAsync();
        var showConfig = NormalizeShowConfig(settings.Show, acts, eventItem.Title);
        return Ok(showConfig);
    }

    [HttpGet("show/configure/validation")]
    public async Task<ActionResult<TalentShowReadinessValidationDTO>> GetShowReadinessValidation(int eventId)
    {
        var configResult = await GetShowConfiguration(eventId);
        if (configResult.Result is NotFoundObjectResult)
        {
            return NotFound("Event not found.");
        }

        var config = configResult.Value ?? new TalentShowShowConfigDTO();
        return Ok(config.ReadinessValidation);
    }

    [HttpPost("show/configure")]
    public async Task<IActionResult> SaveShowConfiguration(int eventId, [FromBody] TalentShowShowConfigDTO dto)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var acts = await _db.TalentShowActs
            .Where(a => a.EventId == eventId && a.SelectedForShow)
            .OrderBy(a => a.OrderIndex)
            .ToListAsync();

        var control = await GetOrCreateTalentShowDirectorControlAsync(eventItem);
        var settings = ParseDirectorSettings(control.SettingsJson);
        settings.Show = NormalizeShowConfig(dto, acts, eventItem.Title);

        control.SettingsJson = JsonSerializer.Serialize(settings, JsonOptions);
        await _db.SaveChangesAsync();
        await BroadcastShowRealtimeAsync(eventId, settings.Show);

        return NoContent();
    }

    [HttpGet("show/runtime")]
    public async Task<ActionResult<TalentShowRuntimeStateDTO>> GetShowRuntimeState(int eventId)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var control = await GetTalentShowDirectorControlAsync(eventId);
        var settings = control == null
            ? new TalentShowDirectorSettings()
            : ParseDirectorSettings(control.SettingsJson);
        var acts = await _db.TalentShowActs
            .Where(a => a.EventId == eventId && a.SelectedForShow)
            .OrderBy(a => a.OrderIndex)
            .ToListAsync();

        var showConfig = NormalizeShowConfig(settings.Show, acts, eventItem.Title);
        ApplyLiveCountdownTransition(showConfig);

        if (control != null)
        {
            settings.Show = showConfig;
            control.SettingsJson = JsonSerializer.Serialize(settings, JsonOptions);
            await _db.SaveChangesAsync();
            await BroadcastShowRealtimeAsync(eventId, settings.Show);
        }

        return Ok(ToShowRuntime(showConfig));
    }

    [HttpPost("show/live/status")]
    public async Task<ActionResult<TalentShowRuntimeStateDTO>> SetShowLiveStatus(int eventId, [FromBody] TalentShowLiveStatusTransitionDTO dto)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var control = await GetOrCreateTalentShowDirectorControlAsync(eventItem);
        var settings = ParseDirectorSettings(control.SettingsJson);
        var acts = await _db.TalentShowActs
            .Where(a => a.EventId == eventId && a.SelectedForShow)
            .OrderBy(a => a.OrderIndex)
            .ToListAsync();
        var showConfig = NormalizeShowConfig(settings.Show, acts, eventItem.Title);

        if (!showConfig.ConfigStatus.Equals(TalentShowConfigStatus.Ready, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Live workspace is locked until Config Status is Ready.");
        }

        var targetStatus = TalentShowLiveStatus.All.FirstOrDefault(status =>
            status.Equals(dto.TargetStatus?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (targetStatus == null)
        {
            return BadRequest("Invalid live status.");
        }

        showConfig.LiveCountdownTargetUtc = null;
        if (targetStatus == TalentShowLiveStatus.Live &&
            showConfig.LiveStatus.Equals(TalentShowLiveStatus.StandBy, StringComparison.OrdinalIgnoreCase) &&
            dto.CountdownSeconds > 0)
        {
            showConfig.LiveCountdownTargetUtc = DateTime.UtcNow.AddSeconds(dto.CountdownSeconds);
        }
        else
        {
            showConfig.LiveStatus = targetStatus;
        }

        showConfig.IsLiveMode = showConfig.LiveStatus.Equals(TalentShowLiveStatus.Live, StringComparison.OrdinalIgnoreCase);
        if (showConfig.IsLiveMode && !showConfig.ShowStartTime.HasValue)
        {
            showConfig.ShowStartTime = DateTime.UtcNow;
        }

        settings.Show = showConfig;
        control.SettingsJson = JsonSerializer.Serialize(settings, JsonOptions);
        await _db.SaveChangesAsync();
        await BroadcastShowRealtimeAsync(eventId, settings.Show);
        if (targetStatus == TalentShowLiveStatus.Done)
        {
            var sessionCode = BuildShowSessionCode(eventId);
            TalentShowHub.RemoveServerState(sessionCode);
            await _hubContext.Clients.Group(sessionCode).SendAsync("SessionCleared");
            await _hubContext.Clients.Group(sessionCode).SendAsync("DeviceSessionCleared");
        }

        return Ok(ToShowRuntime(showConfig));
    }

    [HttpPost("show/live/segment/start")]
    public async Task<ActionResult<TalentShowRuntimeStateDTO>> StartShowSegment(int eventId, [FromBody] TalentShowSegmentActionDTO dto)
    {
        return await RunSegmentMutation(eventId, config =>
        {
            var segment = config.Segments.FirstOrDefault(s => s.SegmentId == dto.SegmentId);
            if (segment == null)
            {
                return "Segment not found.";
            }

            foreach (var seg in config.Segments.Where(s => s.Status == TalentShowSegmentStatus.Live))
            {
                seg.Status = TalentShowSegmentStatus.Completed;
            }

            segment.Status = TalentShowSegmentStatus.Live;
            config.CurrentSegmentId = segment.SegmentId;
            return null;
        });
    }

    [HttpPost("show/live/segment/end")]
    public async Task<ActionResult<TalentShowRuntimeStateDTO>> EndShowSegment(int eventId, [FromBody] TalentShowSegmentActionDTO dto)
    {
        return await RunSegmentMutation(eventId, config =>
        {
            var segment = config.Segments.FirstOrDefault(s => s.SegmentId == dto.SegmentId || s.SegmentId == config.CurrentSegmentId);
            if (segment == null)
            {
                return "Segment not found.";
            }

            segment.Status = TalentShowSegmentStatus.Completed;
            if (config.CurrentSegmentId == segment.SegmentId)
            {
                config.CurrentSegmentId = 0;
            }
            return null;
        });
    }

    [HttpPost("show/live/segment/next")]
    public async Task<ActionResult<TalentShowRuntimeStateDTO>> NextShowSegment(int eventId)
    {
        return await RunSegmentMutation(eventId, config =>
        {
            var ordered = config.Segments.OrderBy(s => s.OrderIndex).ToList();
            TalentShowSegmentDTO? next;

            if (config.CurrentSegmentId == 0)
            {
                next = ordered.FirstOrDefault(s => s.Status == TalentShowSegmentStatus.Waiting);
            }
            else
            {
                var current = ordered.FirstOrDefault(s => s.SegmentId == config.CurrentSegmentId);
                if (current != null && current.Status == TalentShowSegmentStatus.Live)
                {
                    current.Status = TalentShowSegmentStatus.Completed;
                }
                next = ordered.FirstOrDefault(s => s.OrderIndex > (current?.OrderIndex ?? -1) && s.Status == TalentShowSegmentStatus.Waiting);
            }

            if (next == null)
            {
                return "No next segment available.";
            }

            next.Status = TalentShowSegmentStatus.Live;
            config.CurrentSegmentId = next.SegmentId;
            return null;
        });
    }

    [HttpPost("show/live/segment/skip")]
    public async Task<ActionResult<TalentShowRuntimeStateDTO>> SkipShowSegment(int eventId, [FromBody] TalentShowSegmentActionDTO dto)
    {
        return await RunSegmentMutation(eventId, config =>
        {
            var segment = config.Segments.FirstOrDefault(s => s.SegmentId == dto.SegmentId);
            if (segment == null)
            {
                return "Segment not found.";
            }

            segment.Status = TalentShowSegmentStatus.Skipped;
            if (config.CurrentSegmentId == segment.SegmentId)
            {
                config.CurrentSegmentId = 0;
            }
            return null;
        });
    }

    [HttpPost("show/live/performer/update")]
    public async Task<ActionResult<TalentShowRuntimeStateDTO>> UpdateShowPerformerStatus(int eventId, [FromBody] TalentShowPerformerStatusUpdateDTO dto)
    {
        return await RunSegmentMutation(eventId, config =>
        {
            var normalizedStatus = new[] { "StandBy", "OnDeck", "Performing", "Completed" }
                .FirstOrDefault(status => status.Equals(dto.Status?.Trim(), StringComparison.OrdinalIgnoreCase));
            if (normalizedStatus == null)
            {
                return "Invalid performer status.";
            }

            var entry = config.PerformerQueue.FirstOrDefault(item =>
                (dto.ActId.HasValue && item.ActId == dto.ActId) ||
                (!string.IsNullOrWhiteSpace(dto.PerformerName) && !string.IsNullOrWhiteSpace(dto.ActTitle) &&
                 item.PerformerName.Equals(dto.PerformerName, StringComparison.OrdinalIgnoreCase) &&
                 item.ActTitle.Equals(dto.ActTitle, StringComparison.OrdinalIgnoreCase)));

            if (entry == null)
            {
                entry = new TalentShowPerformerQueueEntryDTO
                {
                    ActId = dto.ActId,
                    PerformerName = dto.PerformerName?.Trim() ?? string.Empty,
                    ActTitle = dto.ActTitle?.Trim() ?? string.Empty
                };
                config.PerformerQueue.Add(entry);
            }

            entry.Status = normalizedStatus;
            return null;
        });
    }

    [HttpPost("show/reorder")]
    public async Task<ActionResult<TalentShowRuntimeStateDTO>> ReorderShowSegments(int eventId, [FromBody] TalentShowSegmentReorderDTO dto)
    {
        return await RunSegmentMutation(eventId, config =>
        {
            var reordered = new List<TalentShowSegmentDTO>();
            foreach (var segmentId in dto.OrderedSegmentIds)
            {
                var segment = config.Segments.FirstOrDefault(s => s.SegmentId == segmentId);
                if (segment != null)
                {
                    reordered.Add(segment);
                }
            }

            if (!reordered.Any())
            {
                return "No segments to reorder.";
            }

            for (var i = 0; i < reordered.Count; i++)
            {
                reordered[i].OrderIndex = i + 1;
            }
            config.Segments = reordered;
            return null;
        });
    }

    [HttpPost("show/insert")]
    public async Task<ActionResult<TalentShowRuntimeStateDTO>> InsertShowSegment(int eventId, [FromBody] TalentShowSegmentInsertDTO dto)
    {
        return await RunSegmentMutation(eventId, config =>
        {
            var segmentType = TalentShowSegmentType.All.FirstOrDefault(type =>
                type.Equals(dto.SegmentType?.Trim(), StringComparison.OrdinalIgnoreCase));
            if (segmentType == null)
            {
                return "Invalid segment type.";
            }

            var newSegmentId = (config.Segments.Select(s => s.SegmentId).DefaultIfEmpty(0).Max()) + 1;
            var newSegment = new TalentShowSegmentDTO
            {
                SegmentId = newSegmentId,
                SegmentType = segmentType,
                Title = string.IsNullOrWhiteSpace(dto.Title) ? $"{segmentType} Segment" : dto.Title.Trim(),
                OrderIndex = Math.Max(1, dto.InsertAtIndex),
                DisplayInstructions = new TalentShowDisplayInstructionsDTO
                {
                    HostTeleprompter = dto.HostTeleprompter ?? string.Empty,
                    Backstage = dto.Backstage ?? string.Empty,
                    MainBoard = dto.Title ?? string.Empty,
                    TimerPresetSeconds = dto.DurationSeconds <= 0 ? 180 : dto.DurationSeconds
                },
                MediaFile = dto.MediaFile ?? string.Empty,
                ExpectedDurationSeconds = dto.DurationSeconds <= 0 ? 180 : dto.DurationSeconds,
                Status = TalentShowSegmentStatus.Waiting,
                HostText = dto.HostTeleprompter ?? string.Empty,
                Notes = dto.Backstage ?? string.Empty
            };

            var insertAt = Math.Clamp(dto.InsertAtIndex, 0, config.Segments.Count);
            config.Segments.Insert(insertAt, newSegment);
            for (var i = 0; i < config.Segments.Count; i++)
            {
                config.Segments[i].OrderIndex = i + 1;
            }
            return null;
        });
    }

    [HttpPost("show/vote/judge")]
    public async Task<IActionResult> SubmitJudgeVote(int eventId, [FromBody] TalentShowJudgeVoteDTO dto)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var vote = new TalentShowVote
        {
            EventId = eventId,
            ActId = dto.ActId,
            DeviceOrUserId = dto.JudgeId.ToString(),
            VoterName = string.Empty,
            IsJudge = true,
            TalentScore = Math.Clamp(dto.Score, 1, 5),
            StagePresenceScore = Math.Clamp(dto.Score, 1, 5),
            CreativityScore = Math.Clamp(dto.Score, 1, 5),
            CrowdEngagementScore = Math.Clamp(dto.Score, 1, 5),
            TimestampUtc = DateTime.UtcNow
        };

        vote.TotalScore = (vote.TalentScore + vote.StagePresenceScore + vote.CreativityScore + vote.CrowdEngagementScore) / 4;

        _db.TalentShowVotes.Add(vote);
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("show/voting/control")]
    public async Task<ActionResult<TalentShowRuntimeStateDTO>> ControlShowVoting(int eventId, [FromBody] TalentShowVotingControlDTO dto)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var control = await GetOrCreateTalentShowDirectorControlAsync(eventItem);
        var settings = ParseDirectorSettings(control.SettingsJson);
        var acts = await _db.TalentShowActs
            .Where(a => a.EventId == eventId && a.SelectedForShow)
            .OrderBy(a => a.OrderIndex)
            .ToListAsync();
        var showConfig = NormalizeShowConfig(settings.Show, acts, eventItem.Title);

        if (dto.OpenJudgesVoting) showConfig.ShowSettings.JudgesVotingEnabled = true;
        if (dto.OpenAudienceVoting) showConfig.ShowSettings.AudienceVotingEnabled = true;
        if (dto.CloseVoting)
        {
            showConfig.ShowSettings.JudgesVotingEnabled = false;
            showConfig.ShowSettings.AudienceVotingEnabled = false;
        }
        showConfig.EnableJudgesVoting = showConfig.ShowSettings.JudgesVotingEnabled;
        showConfig.EnableAudienceVoting = showConfig.ShowSettings.AudienceVotingEnabled;

        settings.Show = showConfig;
        control.SettingsJson = JsonSerializer.Serialize(settings, JsonOptions);
        await _db.SaveChangesAsync();
        await BroadcastShowRealtimeAsync(eventId, settings.Show);

        return Ok(ToShowRuntime(showConfig));
    }

    private static TalentShowRuntimeStateDTO ToShowRuntime(TalentShowShowConfigDTO config)
    {
        return new TalentShowRuntimeStateDTO
        {
            ConfigStatus = config.ConfigStatus,
            LiveStatus = config.LiveStatus,
            CurrentSegmentId = config.CurrentSegmentId,
            Segments = config.Segments,
            PerformerQueue = config.PerformerQueue,
            ShowSettings = config.ShowSettings,
            IsLiveMode = config.IsLiveMode,
            ShowStartTime = config.ShowStartTime,
            JudgesVotingOpen = config.ShowSettings.JudgesVotingEnabled,
            AudienceVotingOpen = config.ShowSettings.AudienceVotingEnabled,
            LiveCountdownTargetUtc = config.LiveCountdownTargetUtc
        };
    }

    private static void ApplyLiveCountdownTransition(TalentShowShowConfigDTO config)
    {
        if (config.LiveCountdownTargetUtc.HasValue && config.LiveCountdownTargetUtc.Value <= DateTime.UtcNow)
        {
            config.LiveCountdownTargetUtc = null;
            config.LiveStatus = TalentShowLiveStatus.Live;
            config.IsLiveMode = true;
            config.ShowStartTime ??= DateTime.UtcNow;
        }
    }

    private async Task<ActionResult<TalentShowRuntimeStateDTO>> RunSegmentMutation(int eventId, Func<TalentShowShowConfigDTO, string?> mutate)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var control = await GetOrCreateTalentShowDirectorControlAsync(eventItem);
        var settings = ParseDirectorSettings(control.SettingsJson);
        var acts = await _db.TalentShowActs
            .Where(a => a.EventId == eventId && a.SelectedForShow)
            .OrderBy(a => a.OrderIndex)
            .ToListAsync();
        var showConfig = NormalizeShowConfig(settings.Show, acts, eventItem.Title);

        var error = mutate(showConfig);
        if (!string.IsNullOrWhiteSpace(error))
        {
            return BadRequest(error);
        }

        showConfig.IsLiveMode = showConfig.LiveStatus.Equals(TalentShowLiveStatus.Live, StringComparison.OrdinalIgnoreCase);
        settings.Show = NormalizeShowConfig(showConfig, acts, eventItem.Title);
        control.SettingsJson = JsonSerializer.Serialize(settings, JsonOptions);
        await _db.SaveChangesAsync();
        await BroadcastShowRealtimeAsync(eventId, settings.Show);
        return Ok(ToShowRuntime(settings.Show));
    }

    private async Task BroadcastShowRealtimeAsync(int eventId, TalentShowShowConfigDTO showConfig)
    {
        var sessionCode = BuildShowSessionCode(eventId);
        var realtime = ToShowRealtimeState(showConfig, sessionCode);
        TalentShowHub.UpsertServerState(realtime);
        await _hubContext.Clients.Group(sessionCode).SendAsync("ShowStateUpdated", realtime);
    }

    private static string BuildShowSessionCode(int eventId)
    {
        var raw = $"SHOW-{eventId}";
        var normalized = new string(raw.ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
        return string.IsNullOrWhiteSpace(normalized) ? $"SHOW{eventId}" : normalized;
    }

    private static TalentShowRealtimeState ToShowRealtimeState(TalentShowShowConfigDTO config, string sessionCode)
    {
        var sortedSegments = config.Segments.OrderBy(segment => segment.OrderIndex).ToList();
        var currentSegment = sortedSegments.FirstOrDefault(segment => segment.SegmentId == config.CurrentSegmentId);
        var currentState = config.LiveStatus switch
        {
            var status when status.Equals(TalentShowLiveStatus.Live, StringComparison.OrdinalIgnoreCase)
                => currentSegment?.SegmentType switch
                {
                    var type when type == TalentShowSegmentType.HostTalk => TalentShowLifecycleState.HostTalk,
                    var type when type == TalentShowSegmentType.SponsorAd => TalentShowLifecycleState.HostTalk,
                    var type when type == TalentShowSegmentType.Intermission => TalentShowLifecycleState.Intermission,
                    var type when type == TalentShowSegmentType.Awards => TalentShowLifecycleState.PostShow,
                    _ => TalentShowLifecycleState.ActShow
                },
            var status when status.Equals(TalentShowLiveStatus.WrapUp, StringComparison.OrdinalIgnoreCase) => TalentShowLifecycleState.PostShow,
            var status when status.Equals(TalentShowLiveStatus.Done, StringComparison.OrdinalIgnoreCase) => TalentShowLifecycleState.PostShow,
            _ => TalentShowLifecycleState.PreShow
        };

        var scheduleItems = sortedSegments.Select((segment, index) => new TalentShowScheduleItem
        {
            Order = index + 1,
            PerformerName = ResolvePerformerName(config, segment),
            ActTitle = segment.Title,
            Category = segment.SegmentType,
            DurationMinutes = Math.Max(1, segment.ExpectedDurationSeconds / 60),
            IntroLine = string.IsNullOrWhiteSpace(segment.IntroText) ? segment.HostText : segment.IntroText,
            OutroLine = segment.Notes
        }).ToList();

        var currentIndex = sortedSegments.FindIndex(segment => segment.SegmentId == config.CurrentSegmentId);
        return new TalentShowRealtimeState
        {
            ShowName = "Talent Show",
            SessionCode = sessionCode,
            ConfigStatus = config.ConfigStatus,
            LiveStatus = config.LiveStatus,
            CurrentState = currentState,
            CurrentSegmentType = currentSegment?.SegmentType ?? string.Empty,
            IsLiveMode = config.LiveStatus.Equals(TalentShowLiveStatus.Live, StringComparison.OrdinalIgnoreCase),
            OverallState = config.LiveStatus switch
            {
                var status when status.Equals(TalentShowLiveStatus.Live, StringComparison.OrdinalIgnoreCase) => TalentShowOverallState.Live,
                var status when status.Equals(TalentShowLiveStatus.WrapUp, StringComparison.OrdinalIgnoreCase) => TalentShowOverallState.PostShow,
                var status when status.Equals(TalentShowLiveStatus.Done, StringComparison.OrdinalIgnoreCase) => TalentShowOverallState.PostShow,
                _ => TalentShowOverallState.PreShow
            },
            LiveSubState = currentState == TalentShowLifecycleState.HostTalk ? TalentShowLiveSubState.HostTalk : TalentShowLiveSubState.ActShow,
            NextLiveSubState = TalentShowLiveSubState.ActShow,
            LiveCountdownTargetUtc = config.LiveCountdownTargetUtc,
            SegmentStartedAtUtc = DateTime.UtcNow,
            EstimatedSegmentMinutes = Math.Max(1, (currentSegment?.ExpectedDurationSeconds ?? 180) / 60),
            JudgesVotingEnabled = config.ShowSettings.JudgesVotingEnabled,
            AudienceVotingEnabled = config.ShowSettings.AudienceVotingEnabled,
            HostNames = config.ShowSettings.HostNames.ToList(),
            CurrentIndex = currentIndex,
            UpdatedAtUtc = DateTime.UtcNow,
            Items = scheduleItems,
            Presentation = new TalentShowPresentationSettings
            {
                WelcomeMessage = config.LiveStatus.Equals(TalentShowLiveStatus.StandBy, StringComparison.OrdinalIgnoreCase)
                    ? "Show starting soon"
                    : config.ConfigStatus.Equals(TalentShowConfigStatus.Ready, StringComparison.OrdinalIgnoreCase)
                        ? "Welcome to the Talent Show!"
                        : "Display Connected — Setup Mode",
                IntermissionMessage = "Intermission",
                FooterMessage = config.ConfigStatus.Equals(TalentShowConfigStatus.Ready, StringComparison.OrdinalIgnoreCase)
                    ? "Ready Mode"
                    : "Setup Mode",
                ThemeClass = config.ConfigStatus.Equals(TalentShowConfigStatus.Ready, StringComparison.OrdinalIgnoreCase) ? "theme-ready" : "theme-setup",
                ThemeLabel = config.ShowSettings.ShowTheme,
                BackgroundImageUrl = config.ShowSettings.ShowBackgroundImageUrl,
                SetupTemplateMarkdown = config.ShowSettings.SetupTemplateMarkdown,
                ReadyTemplateMarkdown = config.ShowSettings.ReadyTemplateMarkdown,
                PreShowTemplateMarkdown = config.ShowSettings.PreShowTemplateMarkdown,
                StandByTemplateMarkdown = config.ShowSettings.StandByTemplateMarkdown,
                LiveStartTemplateMarkdown = config.ShowSettings.LiveStartTemplateMarkdown,
                WrapUpTemplateMarkdown = config.ShowSettings.WrapUpTemplateMarkdown,
                DoneTemplateMarkdown = config.ShowSettings.DoneTemplateMarkdown
            }
        };
    }

    private static string ResolvePerformerName(TalentShowShowConfigDTO config, TalentShowSegmentDTO segment)
    {
        if (segment.ActId.HasValue)
        {
            var performer = config.PerformerQueue.FirstOrDefault(item => item.ActId == segment.ActId);
            if (!string.IsNullOrWhiteSpace(performer?.PerformerName))
            {
                return performer.PerformerName;
            }
        }

        return segment.SegmentType == TalentShowSegmentType.Act
            ? "Performer"
            : segment.SegmentType;
    }

    private static TalentShowShowConfigDTO NormalizeShowConfig(TalentShowShowConfigDTO? config, IReadOnlyList<TalentShowAct> selectedActs, string defaultTitle)
    {
        var normalized = config ?? new TalentShowShowConfigDTO();

        normalized.ConfigStatus = TalentShowConfigStatus.All.FirstOrDefault(status =>
            status.Equals(normalized.ConfigStatus?.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? TalentShowConfigStatus.Setup;

        normalized.LiveStatus = TalentShowLiveStatus.All.FirstOrDefault(status =>
            status.Equals(normalized.LiveStatus?.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? TalentShowLiveStatus.PreShow;

        normalized.ShowSettings ??= new TalentShowShowSettingsDTO();
        normalized.ShowSettings.ScoringScale = normalized.ShowSettings.ScoringScale <= 0 ? 5 : normalized.ShowSettings.ScoringScale;
        normalized.ShowSettings.HostNames = (normalized.ShowSettings.HostNames ?? [])
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        normalized.ShowSettings.ScoringCategories = (normalized.ShowSettings.ScoringCategories ?? [])
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Select(category => category.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        normalized.ShowSettings.SetupTemplateMarkdown = EnsureTemplateDefault(normalized.ShowSettings.SetupTemplateMarkdown, "# Setup\nDisplay connected. Director is preparing the show.");
        normalized.ShowSettings.ReadyTemplateMarkdown = EnsureTemplateDefault(normalized.ShowSettings.ReadyTemplateMarkdown, "# Ready\n{{Title}}\nHosted by {{HostedBy}}\nStarting soon.");
        normalized.ShowSettings.PreShowTemplateMarkdown = EnsureTemplateDefault(normalized.ShowSettings.PreShowTemplateMarkdown, "# Pre-Show\nWelcome to {{Title}}");
        normalized.ShowSettings.StandByTemplateMarkdown = EnsureTemplateDefault(normalized.ShowSettings.StandByTemplateMarkdown, "# Stand-By\nShow begins in {{Countdown}}");
        normalized.ShowSettings.LiveStartTemplateMarkdown = EnsureTemplateDefault(normalized.ShowSettings.LiveStartTemplateMarkdown, "# Live\nNow on stage: {{ActTitle}}\n{{PerformerNames}}");
        normalized.ShowSettings.WrapUpTemplateMarkdown = EnsureTemplateDefault(normalized.ShowSettings.WrapUpTemplateMarkdown, "# Wrap-Up\nThank you for supporting {{SchoolName}}.");
        normalized.ShowSettings.DoneTemplateMarkdown = EnsureTemplateDefault(normalized.ShowSettings.DoneTemplateMarkdown, "# Done\nThe show has ended.");

        normalized.EnableJudgesVoting = normalized.ShowSettings.JudgesVotingEnabled;
        normalized.EnableAudienceVoting = normalized.ShowSettings.AudienceVotingEnabled;

        normalized.DisplayAssignments = (normalized.DisplayAssignments ?? [])
            .Where(assignment => !string.IsNullOrWhiteSpace(assignment.Role))
            .Select(assignment => new TalentShowDisplayRoleAssignmentDTO
            {
                Role = TalentShowShowDisplayRole.All.FirstOrDefault(role =>
                    role.Equals(assignment.Role.Trim(), StringComparison.OrdinalIgnoreCase)) ?? assignment.Role.Trim(),
                PairingCode = assignment.PairingCode?.Trim() ?? string.Empty,
                DisplayName = assignment.DisplayName?.Trim() ?? string.Empty
            })
            .ToList();

        normalized.Segments ??= [];
        foreach (var segment in normalized.Segments)
        {
            segment.SegmentType = TalentShowSegmentType.All.FirstOrDefault(type =>
                type.Equals(segment.SegmentType?.Trim(), StringComparison.OrdinalIgnoreCase))
                ?? TalentShowSegmentType.CustomSegment;
            segment.Title = string.IsNullOrWhiteSpace(segment.Title) ? $"{segment.SegmentType} Segment" : segment.Title.Trim();
            segment.ExpectedDurationSeconds = segment.ExpectedDurationSeconds <= 0 ? 180 : segment.ExpectedDurationSeconds;
            segment.DisplayInstructions ??= new TalentShowDisplayInstructionsDTO();
            segment.CueList ??= [];
            segment.MediaTriggers ??= [];
            segment.Status = TalentShowSegmentStatus.All.FirstOrDefault(status =>
                status.Equals(segment.Status?.Trim(), StringComparison.OrdinalIgnoreCase))
                ?? TalentShowSegmentStatus.Waiting;
        }

        if (!normalized.Segments.Any())
        {
            normalized.Segments = selectedActs.Select((act, index) => new TalentShowSegmentDTO
            {
                SegmentId = index + 1,
                SegmentType = TalentShowSegmentType.Act,
                ActId = act.Id,
                AttachedActId = act.Id.ToString(),
                Title = act.Title,
                IntroText = act.IntroLine ?? string.Empty,
                HostText = act.IntroLine ?? string.Empty,
                Notes = act.Notes ?? string.Empty,
                OrderIndex = index + 1,
                IsHidden = false,
                DisplayInstructions = new TalentShowDisplayInstructionsDTO
                {
                    HostTeleprompter = act.IntroLine ?? string.Empty,
                    Backstage = act.Notes ?? string.Empty,
                    MainBoard = $"{act.PerformerName} - {act.Title}",
                    TimerPresetSeconds = act.DurationSeconds <= 0 ? 180 : act.DurationSeconds
                },
                MediaFile = act.MediaFilePath,
                ExpectedDurationSeconds = act.DurationSeconds <= 0 ? 180 : act.DurationSeconds,
                Status = TalentShowSegmentStatus.Waiting
            }).ToList();
        }
        normalized.Segments = normalized.Segments.OrderBy(segment => segment.OrderIndex).ToList();
        for (var i = 0; i < normalized.Segments.Count; i++)
        {
            normalized.Segments[i].OrderIndex = i + 1;
        }

        normalized.PerformerQueue = (normalized.PerformerQueue ?? [])
            .Where(item => !string.IsNullOrWhiteSpace(item.PerformerName) || !string.IsNullOrWhiteSpace(item.ActTitle))
            .Select(item => new TalentShowPerformerQueueEntryDTO
            {
                ActId = item.ActId,
                PerformerName = item.PerformerName?.Trim() ?? string.Empty,
                ActTitle = item.ActTitle?.Trim() ?? string.Empty,
                Status = new[] { "StandBy", "OnDeck", "Performing", "Completed" }
                    .FirstOrDefault(status => status.Equals(item.Status?.Trim(), StringComparison.OrdinalIgnoreCase))
                    ?? "StandBy"
            })
            .ToList();
        if (!normalized.PerformerQueue.Any())
        {
            normalized.PerformerQueue = selectedActs.Select(act => new TalentShowPerformerQueueEntryDTO
            {
                ActId = act.Id,
                PerformerName = act.PerformerName,
                ActTitle = act.Title,
                Status = "StandBy"
            }).ToList();
        }

        normalized.ReadinessValidation = ValidateShowReadiness(normalized);
        normalized.IsLiveMode = normalized.LiveStatus.Equals(TalentShowLiveStatus.Live, StringComparison.OrdinalIgnoreCase);
        normalized.ShowStartTime ??= normalized.IsLiveMode ? DateTime.UtcNow : null;

        if (string.IsNullOrWhiteSpace(defaultTitle))
        {
            defaultTitle = "Talent Show";
        }

        return normalized;
    }

    private static string EnsureTemplateDefault(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static TalentShowReadinessValidationDTO ValidateShowReadiness(TalentShowShowConfigDTO config)
    {
        var errors = new List<string>();

        if (!config.ShowSettings.HostNames.Any())
        {
            errors.Add("At least one Host is required.");
        }

        if (!config.Segments.Any())
        {
            errors.Add("Script / Timeline must include at least one segment.");
        }

        if (!config.Segments.Any(segment => segment.SegmentType == TalentShowSegmentType.Act))
        {
            errors.Add("At least one Act is required.");
        }

        var hasMainDisplay = config.DisplayAssignments.Any(assignment =>
            assignment.Role.Equals(TalentShowShowDisplayRole.MainDisplay, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(assignment.PairingCode));
        var hasHostPrompt = config.DisplayAssignments.Any(assignment =>
            assignment.Role.Equals(TalentShowShowDisplayRole.HostPrompt, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(assignment.PairingCode));
        if (!hasMainDisplay || !hasHostPrompt)
        {
            errors.Add("Displays must include MainDisplay and Host Prompt assignments.");
        }

        if (config.ShowSettings.JudgesVotingEnabled)
        {
            if (!config.ShowSettings.ScoringCategories.Any())
            {
                errors.Add("Judges Voting requires scoring categories.");
            }

            if (config.ShowSettings.ScoringScale <= 0)
            {
                errors.Add("Judges Voting requires a valid scoring scale.");
            }
        }

        if (config.ShowSettings.AudienceVotingEnabled &&
            (config.ShowSettings.ScoringScale <= 0 || !config.ShowSettings.ScoringCategories.Any()))
        {
            errors.Add("Audience Voting requires valid voting configuration.");
        }

        return new TalentShowReadinessValidationDTO
        {
            IsReadySuggested = !errors.Any(),
            Errors = errors
        };
    }
}
