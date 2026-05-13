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
}
