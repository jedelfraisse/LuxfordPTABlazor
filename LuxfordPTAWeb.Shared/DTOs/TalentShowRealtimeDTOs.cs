namespace LuxfordPTAWeb.Shared.DTOs;

public class TalentShowRehearsalPackage
{
    public string ShowName { get; set; } = string.Empty;
    public string SessionCode { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public DateTime ShowDate { get; set; }
    public List<TalentShowScheduleItem> Items { get; set; } = new();
    public TalentShowPresentationSettings Presentation { get; set; } = new();
}

public class TalentShowScheduleItem
{
    public int Order { get; set; }
    public string PerformerName { get; set; } = string.Empty;
    public string ActTitle { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string IntroLine { get; set; } = string.Empty;
    public string OutroLine { get; set; } = string.Empty;
}

public class TalentShowPresentationSettings
{
    public string WelcomeMessage { get; set; } = string.Empty;
    public string FooterMessage { get; set; } = string.Empty;
    public string IntermissionMessage { get; set; } = string.Empty;
    public string ThemeClass { get; set; } = string.Empty;
}

public class TalentShowRealtimeState
{
    public string ShowName { get; set; } = string.Empty;
    public string SessionCode { get; set; } = string.Empty;
    public string CurrentState { get; set; } = TalentShowLifecycleState.PreShow;
    public bool IsLiveMode { get; set; }
    public string OverallState { get; set; } = TalentShowOverallState.PreShow;
    public string LiveSubState { get; set; } = TalentShowLiveSubState.HostTalk;
    public string NextLiveSubState { get; set; } = TalentShowLiveSubState.ActShow;
    public DateTime SegmentStartedAtUtc { get; set; } = DateTime.UtcNow;
    public int EstimatedSegmentMinutes { get; set; } = 5;
    public int CurrentIndex { get; set; } = -1;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<TalentShowScheduleItem> Items { get; set; } = new();
    public TalentShowPresentationSettings Presentation { get; set; } = new();
    public List<TalentShowVoteSubmission> Votes { get; set; } = new();
}

public static class TalentShowLifecycleState
{
    public const string Planning = "Planning";
    public const string TryOuts = "TryOuts";
    public const string Rehearsal = "Rehearsal";
    public const string PreShow = "PreShow";
    public const string HostTalk = "HostTalk";
    public const string ActShow = "ActShow";
    public const string Intermission = "Intermission";
    public const string PostShow = "PostShow";

    public static readonly string[] All =
    [
        Planning,
        TryOuts,
        Rehearsal,
        PreShow,
        HostTalk,
        ActShow,
        Intermission,
        PostShow
    ];
}

public static class TalentShowOverallState
{
    public const string PreShow = "PreShow";
    public const string Live = "Live";
    public const string PostShow = "PostShow";

    public static readonly string[] All =
    [
        PreShow,
        Live,
        PostShow
    ];
}

public static class TalentShowLiveSubState
{
    public const string HostTalk = "HostTalk";
    public const string ActShow = "ActShow";
    public const string PauseIntermission = "PauseIntermission";

    public static readonly string[] All =
    [
        HostTalk,
        ActShow,
        PauseIntermission
    ];
}

public static class TalentShowDisplayRole
{
    public const string MainBoard = "MainBoard";
    public const string BackstageDirector = "BackstageDirector";
    public const string JudgesVote = "JudgesVote";
    public const string AudienceVote = "AudienceVote";

    public static readonly string[] All =
    [
        MainBoard,
        BackstageDirector,
        JudgesVote,
        AudienceVote
    ];
}

public class TalentShowDeviceRegistrationResult
{
    public string DeviceId { get; set; } = string.Empty;
    public string PairingCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AssignedSessionCode { get; set; } = string.Empty;
    public string AssignedDisplayRole { get; set; } = string.Empty;
    public bool IsAssigned => !string.IsNullOrWhiteSpace(AssignedSessionCode);
}

public class TalentShowDeviceRegistrationRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public string ExistingDeviceId { get; set; } = string.Empty;
    public string ExistingPairingCode { get; set; } = string.Empty;
    public bool ForceNewCode { get; set; }
}

public class TalentShowConnectedDevice
{
    public string DeviceId { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public string PairingCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AssignedSessionCode { get; set; } = string.Empty;
    public string AssignedDisplayRole { get; set; } = string.Empty;
    public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;
    public bool IsAssigned => !string.IsNullOrWhiteSpace(AssignedSessionCode);
}

public class TalentShowDeviceAssignment
{
    public string PairingCode { get; set; } = string.Empty;
    public string SessionCode { get; set; } = string.Empty;
    public string DisplayRole { get; set; } = string.Empty;
}

public class TalentShowVoteSubmission
{
    public string VoterName { get; set; } = string.Empty;
    public bool IsJudge { get; set; }
    public int ActOrder { get; set; }
    public int TalentScore { get; set; } = 3;
    public int StagePresenceScore { get; set; } = 3;
    public int CreativityScore { get; set; } = 3;
    public int CrowdEngagementScore { get; set; } = 3;
    public int TotalScore => TalentScore + StagePresenceScore + CreativityScore + CrowdEngagementScore;
    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
}
