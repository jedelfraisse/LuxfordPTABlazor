using System.Collections.Concurrent;
using LuxfordPTAWeb.Shared.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace LuxfordPTAWeb.Hubs;

public class TalentShowHub : Hub
{
    private static readonly ConcurrentDictionary<string, TalentShowRealtimeState> SessionStates =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, TalentShowConnectedDevice> DevicesByConnection =
        new(StringComparer.Ordinal);
    private const string ControllerGroupName = "talent-show-controllers";

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
        state.OverallState = NormalizeOverallState(state.OverallState);
        state.LiveSubState = NormalizeLiveSubState(state.LiveSubState);
        state.NextLiveSubState = NormalizeLiveSubState(state.NextLiveSubState);
        state.UpdatedAtUtc = DateTime.UtcNow;
        SessionStates[normalizedCode] = state;

        await Clients.Group(normalizedCode).SendAsync("ShowStateUpdated", state);
    }

    public async Task SubmitVote(string sessionCode, TalentShowVoteSubmission vote)
    {
        var normalizedCode = NormalizeSessionCode(sessionCode);
        if (!SessionStates.TryGetValue(normalizedCode, out var state))
        {
            throw new HubException("Session not found.");
        }

        var safeVote = new TalentShowVoteSubmission
        {
            VoterName = string.IsNullOrWhiteSpace(vote.VoterName) ? "Anonymous" : vote.VoterName.Trim(),
            IsJudge = vote.IsJudge,
            ActOrder = vote.ActOrder,
            TalentScore = NormalizeScore(vote.TalentScore),
            StagePresenceScore = NormalizeScore(vote.StagePresenceScore),
            CreativityScore = NormalizeScore(vote.CreativityScore),
            CrowdEngagementScore = NormalizeScore(vote.CrowdEngagementScore),
            SubmittedAtUtc = DateTime.UtcNow
        };

        state.Votes.Add(safeVote);
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

    public async Task SubscribeController()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ControllerGroupName);
        await Clients.Caller.SendAsync("DeviceRegistryUpdated", GetDeviceRegistrySnapshot());
    }

    public async Task<TalentShowDeviceRegistrationResult> RegisterDevice(string? displayName)
    {
        var normalizedName = string.IsNullOrWhiteSpace(displayName)
            ? $"Display-{DateTime.UtcNow:HHmmss}"
            : displayName.Trim();

        var device = new TalentShowConnectedDevice
        {
            DeviceId = Guid.NewGuid().ToString("N"),
            ConnectionId = Context.ConnectionId,
            PairingCode = GenerateUniquePairingCode(),
            DisplayName = normalizedName,
            LastSeenUtc = DateTime.UtcNow
        };

        DevicesByConnection[Context.ConnectionId] = device;
        await BroadcastDeviceRegistryAsync();

        return new TalentShowDeviceRegistrationResult
        {
            DeviceId = device.DeviceId,
            PairingCode = device.PairingCode,
            DisplayName = device.DisplayName
        };
    }

    public Task<List<TalentShowConnectedDevice>> GetDeviceRegistry()
    {
        return Task.FromResult(GetDeviceRegistrySnapshot());
    }

    public async Task AssignDeviceToSession(string pairingCode, string sessionCode, string displayRole)
    {
        var normalizedPairingCode = NormalizePairingCode(pairingCode);
        var normalizedSessionCode = NormalizeSessionCode(sessionCode);
        var normalizedRole = NormalizeDisplayRole(displayRole);

        var device = DevicesByConnection.Values.FirstOrDefault(d =>
            d.PairingCode.Equals(normalizedPairingCode, StringComparison.OrdinalIgnoreCase));

        if (device == null)
        {
            throw new HubException("Device not found.");
        }

        if (!string.IsNullOrWhiteSpace(device.AssignedSessionCode) &&
            !device.AssignedSessionCode.Equals(normalizedSessionCode, StringComparison.OrdinalIgnoreCase))
        {
            await Groups.RemoveFromGroupAsync(device.ConnectionId, device.AssignedSessionCode);
        }

        await Groups.AddToGroupAsync(device.ConnectionId, normalizedSessionCode);

        device.AssignedSessionCode = normalizedSessionCode;
        device.AssignedDisplayRole = normalizedRole;
        device.LastSeenUtc = DateTime.UtcNow;
        DevicesByConnection[device.ConnectionId] = device;

        var assignment = new TalentShowDeviceAssignment
        {
            PairingCode = device.PairingCode,
            SessionCode = device.AssignedSessionCode,
            DisplayRole = device.AssignedDisplayRole
        };

        await Clients.Client(device.ConnectionId).SendAsync("DeviceAssigned", assignment);

        if (SessionStates.TryGetValue(normalizedSessionCode, out var state))
        {
            await Clients.Client(device.ConnectionId).SendAsync("ShowStateUpdated", state);
        }

        await BroadcastDeviceRegistryAsync();
    }

    public async Task RemoveDeviceAssignment(string pairingCode)
    {
        var normalizedPairingCode = NormalizePairingCode(pairingCode);
        var device = DevicesByConnection.Values.FirstOrDefault(d =>
            d.PairingCode.Equals(normalizedPairingCode, StringComparison.OrdinalIgnoreCase));

        if (device == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(device.AssignedSessionCode))
        {
            await Groups.RemoveFromGroupAsync(device.ConnectionId, device.AssignedSessionCode);
        }

        device.AssignedSessionCode = string.Empty;
        device.AssignedDisplayRole = string.Empty;
        device.LastSeenUtc = DateTime.UtcNow;
        DevicesByConnection[device.ConnectionId] = device;

        await Clients.Client(device.ConnectionId).SendAsync("DeviceAssignmentCleared");
        await BroadcastDeviceRegistryAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (DevicesByConnection.TryRemove(Context.ConnectionId, out _))
        {
            await BroadcastDeviceRegistryAsync();
        }

        await base.OnDisconnectedAsync(exception);
    }

    private static string NormalizeSessionCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return "DEFAULT";
        }

        return new string(code.Trim().ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
    }

    private static string NormalizePairingCode(string code)
    {
        return new string(code.Trim().ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
    }

    private static string NormalizeDisplayRole(string role)
    {
        var match = TalentShowDisplayRole.All
            .FirstOrDefault(r => r.Equals(role?.Trim(), StringComparison.OrdinalIgnoreCase));
        return match ?? TalentShowDisplayRole.MainBoard;
    }

    private static string NormalizeOverallState(string state)
    {
        var match = TalentShowOverallState.All
            .FirstOrDefault(s => s.Equals(state?.Trim(), StringComparison.OrdinalIgnoreCase));
        return match ?? TalentShowOverallState.PreShow;
    }

    private static string NormalizeLiveSubState(string state)
    {
        var match = TalentShowLiveSubState.All
            .FirstOrDefault(s => s.Equals(state?.Trim(), StringComparison.OrdinalIgnoreCase));
        return match ?? TalentShowLiveSubState.HostTalk;
    }

    private static int NormalizeScore(int score)
    {
        if (score < 1) return 1;
        if (score > 5) return 5;
        return score;
    }

    private static string GenerateUniquePairingCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        for (var attempt = 0; attempt < 50; attempt++)
        {
            var code = new string(Enumerable.Range(0, 5)
                .Select(_ => chars[Random.Shared.Next(chars.Length)])
                .ToArray());

            var exists = DevicesByConnection.Values.Any(d =>
                d.PairingCode.Equals(code, StringComparison.OrdinalIgnoreCase));
            if (!exists)
            {
                return code;
            }
        }

        return Guid.NewGuid().ToString("N")[..5].ToUpperInvariant();
    }

    private static List<TalentShowConnectedDevice> GetDeviceRegistrySnapshot()
    {
        return DevicesByConnection.Values
            .OrderBy(d => d.DisplayName)
            .ThenBy(d => d.PairingCode)
            .Select(d => new TalentShowConnectedDevice
            {
                DeviceId = d.DeviceId,
                ConnectionId = d.ConnectionId,
                PairingCode = d.PairingCode,
                DisplayName = d.DisplayName,
                AssignedSessionCode = d.AssignedSessionCode,
                AssignedDisplayRole = d.AssignedDisplayRole,
                LastSeenUtc = d.LastSeenUtc
            })
            .ToList();
    }

    private Task BroadcastDeviceRegistryAsync()
    {
        return Clients.Group(ControllerGroupName).SendAsync("DeviceRegistryUpdated", GetDeviceRegistrySnapshot());
    }
}
