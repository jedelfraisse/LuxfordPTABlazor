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
    public string ConfigStatus { get; set; } = TalentShowConfigStatus.Setup;
    public string LiveStatus { get; set; } = TalentShowLiveStatus.PreShow;
    public string CurrentState { get; set; } = TalentShowLifecycleState.PreShow;
    public string CurrentSegmentType { get; set; } = string.Empty;
    public bool IsLiveMode { get; set; }
    public string OverallState { get; set; } = TalentShowOverallState.PreShow;
    public string LiveSubState { get; set; } = TalentShowLiveSubState.HostTalk;
    public string NextLiveSubState { get; set; } = TalentShowLiveSubState.ActShow;
    public DateTime? LiveCountdownTargetUtc { get; set; }
    public DateTime SegmentStartedAtUtc { get; set; } = DateTime.UtcNow;
    public int EstimatedSegmentMinutes { get; set; } = 5;
    public bool JudgesVotingEnabled { get; set; }
    public bool AudienceVotingEnabled { get; set; }
    public List<string> HostNames { get; set; } = new();
    public DateTime? TryOutSessionEndUtc { get; set; }
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
    public const string MainDisplay = "MainDisplay";
    public const string BackstagePrompt = "BackstagePrompt";
    public const string FrontstagePrompt = "FrontstagePrompt";
    public const string HostPrompt = "HostPrompt";
    public const string Judges = "Judges";
    public const string Voters = "Voters";
    public const string TryOutsMainBoard = "TryOuts.MainBoard";
    public const string TryOutsActDisplay = "TryOuts.ActDisplay";
    public const string TryOutsJudgePage = "TryOuts.JudgePage";

    public static readonly string[] All =
    [
        MainBoard,
        BackstageDirector,
        JudgesVote,
        AudienceVote,
        MainDisplay,
        BackstagePrompt,
        FrontstagePrompt,
        HostPrompt,
        Judges,
        Voters,
        TryOutsMainBoard,
        TryOutsActDisplay,
        TryOutsJudgePage
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

public class TalentShowDisplayCommand
{
    public string SessionCode { get; set; } = string.Empty;
    public string TargetRole { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
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
