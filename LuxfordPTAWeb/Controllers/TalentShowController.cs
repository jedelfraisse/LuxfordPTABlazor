using System.Text.Json;
using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Shared.DTOs;
using LuxfordPTAWeb.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuxfordPTAWeb.Controllers;

[ApiController]
[Route("api/talentshow/{eventId:int}")]
[Authorize(Roles = "Admin,BoardMember")]
public class TalentShowController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private const string TalentShowDirectorControlType = "TalentShowDirectorControl";

    public TalentShowController(ApplicationDbContext db)
    {
        _db = db;
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
    public async Task<IActionResult> SaveTryOutsConfiguration(int eventId, [FromBody] TalentShowTryOutsConfigDTO dto)
    {
        var eventItem = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (eventItem == null)
        {
            return NotFound("Event not found.");
        }

        var control = await GetOrCreateTalentShowDirectorControlAsync(eventItem);
        var settings = ParseDirectorSettings(control.SettingsJson);
        settings.TryOuts = NormalizeTryOutsConfig(dto);
        control.SettingsJson = JsonSerializer.Serialize(settings);

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
            SlotTime = dto.SlotTime,
            SessionLabel = dto.SessionLabel?.Trim() ?? string.Empty,
            Notes = dto.Notes?.Trim() ?? string.Empty,
            Status = NormalizeTryOutEntryStatus(dto.Status),
            Selected = dto.Selected,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

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
        entry.SessionLabel = dto.SessionLabel?.Trim() ?? string.Empty;
        entry.Notes = dto.Notes?.Trim() ?? string.Empty;
        entry.Status = NormalizeTryOutEntryStatus(dto.Status);
        entry.Selected = dto.Selected;
        entry.UpdatedAtUtc = DateTime.UtcNow;

        if (entry.Selected)
        {
            await EnsureActForTryOutEntryAsync(eventId, entry);
        }

        await _db.SaveChangesAsync();
        return NoContent();
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
        normalized.SessionLabels = (normalized.SessionLabels ?? [])
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Select(label => label.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        normalized.AssignedHelpers = (normalized.AssignedHelpers ?? [])
            .Where(helper => !string.IsNullOrWhiteSpace(helper))
            .Select(helper => helper.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        normalized.SlotLengthMinutes = normalized.SlotLengthMinutes <= 0 ? 5 : normalized.SlotLengthMinutes;
        normalized.BreakMinutes = normalized.BreakMinutes < 0 ? 0 : normalized.BreakMinutes;
        normalized.MaxPerformersPerSession = normalized.MaxPerformersPerSession <= 0 ? 30 : normalized.MaxPerformersPerSession;
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
            return TalentShowTryOutEntryStatus.Scheduled;
        }

        var normalized = status.Trim();
        return TalentShowTryOutEntryStatus.All.FirstOrDefault(s =>
                   s.Equals(normalized, StringComparison.OrdinalIgnoreCase))
               ?? TalentShowTryOutEntryStatus.Scheduled;
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
            MediaFilePath = signup.MediaUpload,
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
            existingEntry.Status = TalentShowTryOutEntryStatus.Scheduled;
            existingEntry.UpdatedAtUtc = DateTime.UtcNow;
            return;
        }

        var newEntry = new TalentShowTryOutEntry
        {
            EventId = eventId,
            SignupId = signup.Id,
            PerformerNames = signup.PerformerNames,
            ActTitle = signup.ActTitle,
            SessionLabel = sessionLabel?.Trim() ?? string.Empty,
            Notes = signup.SpecialRequirements,
            Selected = false,
            Status = TalentShowTryOutEntryStatus.Scheduled,
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
            MediaFilePath = string.Empty,
            Notes = entry.Notes,
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
    }
}
