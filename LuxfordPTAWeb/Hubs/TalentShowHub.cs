using System.Collections.Concurrent;
using LuxfordPTAWeb.Shared.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace LuxfordPTAWeb.Hubs;

public class TalentShowHub : Hub
{
    private static readonly ConcurrentDictionary<string, TalentShowRealtimeState> SessionStates =
        new(StringComparer.OrdinalIgnoreCase);

    public async Task JoinSession(string sessionCode)
    {
        var normalizedCode = NormalizeSessionCode(sessionCode);
        await Groups.AddToGroupAsync(Context.ConnectionId, normalizedCode);

        if (SessionStates.TryGetValue(normalizedCode, out var state))
        {
            await Clients.Caller.SendAsync("ShowStateUpdated", state);
        }
    }

    public async Task PublishState(TalentShowRealtimeState state)
    {
        var normalizedCode = NormalizeSessionCode(state.SessionCode);
        state.SessionCode = normalizedCode;
        state.UpdatedAtUtc = DateTime.UtcNow;
        SessionStates[normalizedCode] = state;

        await Clients.Group(normalizedCode).SendAsync("ShowStateUpdated", state);
    }

    public async Task ClearSession(string sessionCode)
    {
        var normalizedCode = NormalizeSessionCode(sessionCode);
        SessionStates.TryRemove(normalizedCode, out _);
        await Clients.Group(normalizedCode).SendAsync("SessionCleared");
    }

    private static string NormalizeSessionCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return "DEFAULT";
        }

        return new string(code.Trim().ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
    }
}
