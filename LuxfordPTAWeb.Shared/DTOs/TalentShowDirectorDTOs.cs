namespace LuxfordPTAWeb.Shared.DTOs;

public class TalentShowActUpsertDTO
{
    public string PerformerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public int OrderIndex { get; set; }
    public string IntroLine { get; set; } = string.Empty;
    public string OutroLine { get; set; } = string.Empty;
    public string MediaFilePath { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool SelectedForShow { get; set; } = true;
    public string MusicUrl { get; set; } = string.Empty;
    public int MusicStartOffsetSeconds { get; set; }
    public string PerformerNotes { get; set; } = string.Empty;
    public string StageNotes { get; set; } = string.Empty;
    public string LightingNotes { get; set; } = string.Empty;
    public string SoundNotes { get; set; } = string.Empty;
    public string PropsRequired { get; set; } = string.Empty;
    public string HostIntro { get; set; } = string.Empty;
}

public class TalentShowReorderActsDTO
{
    public List<int> OrderedActIds { get; set; } = [];
}

public class TalentShowVoteResultRowDTO
{
    public int ActId { get; set; }
    public string PerformerName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int JudgeCount { get; set; }
    public int AudienceCount { get; set; }
    public double JudgeAverageTotal { get; set; }
    public double AudienceAverageTotal { get; set; }
    public double CombinedAverageTotal { get; set; }
}

public class TalentShowLifecycleTransitionDTO
{
    public string TargetState { get; set; } = string.Empty;
}

public class TalentShowPlanningCategoryDTO
{
    public string CategoryId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}

public class TalentShowPlanningNoteDTO
{
    public string NoteId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}

public class TalentShowPlanningQuestionDTO
{
    public string QuestionId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool Resolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class TalentShowPlanningMeetingDateDTO
{
    public string MeetingDateId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string AddedByUserId { get; set; } = string.Empty;
    public string? LockedToUserId { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class TalentShowPlanningConfigDTO
{
    public List<TalentShowPlanningMeetingDateDTO> MeetingDates { get; set; } = [];
    public List<string> AgendaTemplateItems { get; set; } = [];
    public List<string> AssignedHelpers { get; set; } = [];
    public int MaxActs { get; set; } = 25;
    public bool MaxActsFlexible { get; set; }
    public int? MaxActsMin { get; set; }
    public int? MaxActsMax { get; set; }
    public int JudgeCount { get; set; } = 3;
    public List<string> JudgeNames { get; set; } = [];
    public DateTime? SignupStart { get; set; }
    public DateTime? SignupEnd { get; set; }
    public bool AutoClose { get; set; } = true;
    public string? ExternalSignupUrl { get; set; }
    public List<TalentShowPlanningCategoryDTO> Categories { get; set; } = [];
    public string RulesMarkdown { get; set; } = string.Empty;
    public string? RulesPdfPath { get; set; }
    public bool PublishRules { get; set; }
    public List<TalentShowPlanningNoteDTO> Notes { get; set; } = [];
    public List<TalentShowPlanningQuestionDTO> Questions { get; set; } = [];
}

public class TalentShowGlobalSetupConfigDTO
{
    public string EventTitle { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string BrandingMessage { get; set; } = string.Empty;
    public string DefaultDisplaySettings { get; set; } = string.Empty;
    public string DefaultMediaRules { get; set; } = string.Empty;
    public string DefaultCueRules { get; set; } = string.Empty;
    public string DefaultSegmentBehavior { get; set; } = string.Empty;
    public bool JudgesVotingEnabledByDefault { get; set; } = true;
    public bool AudienceVotingEnabledByDefault { get; set; } = true;
    public bool PublishGlobalInfo { get; set; }
    public int DefaultMaxActs { get; set; } = 25;
    public bool DefaultMaxActsFlexible { get; set; }
    public int? DefaultMaxActsMin { get; set; }
    public int? DefaultMaxActsMax { get; set; }
    public int DefaultJudgeCount { get; set; } = 3;
    public List<string> DefaultJudgeNames { get; set; } = [];
    public DateTime? DefaultSignupStart { get; set; }
    public DateTime? DefaultSignupEnd { get; set; }
    public bool DefaultAutoClose { get; set; } = true;
    public string? DefaultExternalSignupUrl { get; set; }
    public List<string> DefaultCategories { get; set; } = [];
    public string DefaultRulesMarkdown { get; set; } = string.Empty;
    public string? DefaultRulesPdfPath { get; set; }
    public bool DefaultPublishRules { get; set; }
    public string GlobalDisplayBackgroundImageUrl { get; set; } = string.Empty;
}

public class TalentShowSignupSettingsDTO
{
    public DateTime? SignupStart { get; set; }
    public DateTime? SignupEnd { get; set; }
    public bool AutoClose { get; set; } = true;
    public bool ShowCountdown { get; set; } = true;
    public string PublicUrl { get; set; } = string.Empty;
    public string Title { get; set; } = "Talent Show Sign-Ups";
    public string Subtitle { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public string ConfirmationMessage { get; set; } = string.Empty;
}

public class TalentShowSignupUpsertDTO
{
    public string PerformerNames { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public string Teacher { get; set; } = string.Empty;
    public string ActTitle { get; set; } = string.Empty;
    public string ActDescription { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string SpecialRequirements { get; set; } = string.Empty;
    public string MediaUpload { get; set; } = string.Empty;
    public string PickupAdult { get; set; } = string.Empty;
    public string MusicUrl { get; set; } = string.Empty;
}

public class TalentShowSignupReviewDecisionDTO
{
    public string Action { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public bool AllowResubmission { get; set; }
    public string TryOutSession { get; set; } = string.Empty;
}

public class TalentShowTryOutsConfigDTO
{
    public int SlotLengthMinutes { get; set; } = 5;
    public bool ParentSignOutRequired { get; set; }
    public int ScoringScaleMax { get; set; } = 5;
    public string ActiveSessionId { get; set; } = string.Empty;
    public List<TalentShowTryOutScoringFieldDTO> ScoringFields { get; set; } = [];
    public List<TalentShowTryOutSessionDTO> Sessions { get; set; } = [];
}

public class TalentShowTryOutScoringFieldDTO
{
    public string FieldId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int MaxScore { get; set; } = 5;
    public int OrderIndex { get; set; }
}

public class TalentShowTryOutSessionDTO
{
    public string SessionId { get; set; } = string.Empty;
    public DateOnly? Date { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool PublicVisible { get; set; }
    public bool ParentSignOutRequired { get; set; }
}

public class TalentShowTryOutScoreDTO
{
    public string Category { get; set; } = string.Empty;
    public int Value { get; set; }
    public string JudgeId { get; set; } = string.Empty;
    public int PerformerId { get; set; }
}

public class TalentShowJudgeCompletionDTO
{
    public string JudgeId { get; set; } = string.Empty;
    public int PerformerId { get; set; }
    public bool Completed { get; set; }
}

public class TalentShowTryOutEntryUpsertDTO
{
    public int? SignupId { get; set; }
    public string PerformerNames { get; set; } = string.Empty;
    public string ActTitle { get; set; } = string.Empty;
    public string MusicUrl { get; set; } = string.Empty;
    public string PickupAdult { get; set; } = string.Empty;
    public DateTime? SlotTime { get; set; }
    public DateTime? CheckInTimestamp { get; set; }
    public string SessionLabel { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public List<TalentShowTryOutScoreDTO> Scores { get; set; } = [];
    public List<TalentShowJudgeCompletionDTO> JudgeCompletions { get; set; } = [];
    public string Status { get; set; } = string.Empty;
    public bool Selected { get; set; }
}

// ============================================================================
// THE SHOW MODULE DTOs
// ============================================================================

public static class TalentShowSegmentType
{
    public const string Act = "Act";
    public const string HostTalk = "HostTalk";
    public const string Intermission = "Intermission";
    public const string SponsorAd = "SponsorAd";
    public const string Awards = "Awards";
    public const string CustomSegment = "CustomSegment";

    public static List<string> All => new()
    {
        HostTalk, Act, Intermission, SponsorAd, Awards, CustomSegment
    };
}

public static class TalentShowSegmentStatus
{
    public const string Waiting = "Waiting";
    public const string Live = "Live";
    public const string Completed = "Completed";
    public const string Skipped = "Skipped";

    public static List<string> All => new() { Waiting, Live, Completed, Skipped };
}

public class TalentShowDisplayInstructionsDTO
{
    public string HostTeleprompter { get; set; } = string.Empty;
    public string Backstage { get; set; } = string.Empty;
    public string MainBoard { get; set; } = string.Empty;
    public int TimerPresetSeconds { get; set; }
}

public class TalentShowSegmentDTO
{
    public int SegmentId { get; set; }
    public string SegmentType { get; set; } = string.Empty;
    public int? ActId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public bool IsHidden { get; set; }
    public TalentShowDisplayInstructionsDTO DisplayInstructions { get; set; } = new();
    public string MediaFile { get; set; } = string.Empty;
    public List<string> CueList { get; set; } = [];
    public string? TechRequirements { get; set; }
    public int ExpectedDurationSeconds { get; set; }
    public int ActualDurationSeconds { get; set; }
    public bool RunsLong { get; set; }
    public string Status { get; set; } = TalentShowSegmentStatus.Waiting;
    public string IntroText { get; set; } = string.Empty;
    public string HostText { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public List<string> MediaTriggers { get; set; } = [];
    public bool EnableJudgesVoting { get; set; }
    public bool EnableAudienceVoting { get; set; }
    public string AttachedActId { get; set; } = string.Empty;
}

public static class TalentShowConfigStatus
{
    public const string Setup = "Setup";
    public const string Ready = "Ready";

    public static readonly string[] All =
    [
        Setup,
        Ready
    ];
}

public static class TalentShowLiveStatus
{
    public const string PreShow = "Pre-Show";
    public const string StandBy = "Stand-By";
    public const string Live = "Live";
    public const string WrapUp = "Wrap-Up";
    public const string Done = "Done";

    public static readonly string[] All =
    [
        PreShow,
        StandBy,
        Live,
        WrapUp,
        Done
    ];
}

public static class TalentShowShowDisplayRole
{
    public const string MainDisplay = "MainDisplay";
    public const string BackstagePrompt = "BackstagePrompt";
    public const string FrontstagePrompt = "FrontstagePrompt";
    public const string HostPrompt = "HostPrompt";
    public const string Judges = "Judges";
    public const string Voters = "Voters";

    public static readonly string[] All =
    [
        MainDisplay,
        BackstagePrompt,
        FrontstagePrompt,
        HostPrompt,
        Judges,
        Voters
    ];
}

public class TalentShowShowSettingsDTO
{
    public bool JudgesVotingEnabled { get; set; } = true;
    public bool AudienceVotingEnabled { get; set; } = true;
    public List<string> HostNames { get; set; } = [];
    public List<string> ScoringCategories { get; set; } = [];
    public int ScoringScale { get; set; } = 5;
    public string ShowBackgroundImageUrl { get; set; } = string.Empty;
    public string ShowTheme { get; set; } = string.Empty;
    public int DefaultSegmentDurationSeconds { get; set; }
    public bool EnableSponsorRotation { get; set; }
    public string SetupTemplateMarkdown { get; set; } = string.Empty;
    public string ReadyTemplateMarkdown { get; set; } = string.Empty;
    public string PreShowTemplateMarkdown { get; set; } = string.Empty;
    public string StandByTemplateMarkdown { get; set; } = string.Empty;
    public string LiveStartTemplateMarkdown { get; set; } = string.Empty;
    public string WrapUpTemplateMarkdown { get; set; } = string.Empty;
    public string DoneTemplateMarkdown { get; set; } = string.Empty;
}

public class TalentShowDisplayRoleAssignmentDTO
{
    public string Role { get; set; } = string.Empty;
    public string PairingCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class TalentShowPerformerQueueEntryDTO
{
    public string PerformerName { get; set; } = string.Empty;
    public string ActTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? ActId { get; set; }
}

public class TalentShowReadinessValidationDTO
{
    public bool IsReadySuggested { get; set; }
    public List<string> Errors { get; set; } = [];
}

public class TalentShowShowConfigDTO
{
    public string ConfigStatus { get; set; } = TalentShowConfigStatus.Setup;
    public string LiveStatus { get; set; } = TalentShowLiveStatus.PreShow;
    public TalentShowShowSettingsDTO ShowSettings { get; set; } = new();
    public List<TalentShowDisplayRoleAssignmentDTO> DisplayAssignments { get; set; } = [];
    public List<TalentShowPerformerQueueEntryDTO> PerformerQueue { get; set; } = [];
    public List<TalentShowSegmentDTO> Segments { get; set; } = [];
    public int CurrentSegmentId { get; set; }
    public bool EnableJudgesVoting { get; set; } = true;
    public bool EnableAudienceVoting { get; set; } = true;
    public bool IsLiveMode { get; set; }
    public DateTime? ShowStartTime { get; set; }
    public DateTime? ShowStartTimeOverride { get; set; }
    public DateTime? LiveCountdownTargetUtc { get; set; }
    public TalentShowReadinessValidationDTO ReadinessValidation { get; set; } = new();
}

public class TalentShowRuntimeStateDTO
{
    public string ConfigStatus { get; set; } = TalentShowConfigStatus.Setup;
    public string LiveStatus { get; set; } = TalentShowLiveStatus.PreShow;
    public int CurrentSegmentId { get; set; }
    public List<TalentShowSegmentDTO> Segments { get; set; } = [];
    public List<TalentShowPerformerQueueEntryDTO> PerformerQueue { get; set; } = [];
    public TalentShowShowSettingsDTO ShowSettings { get; set; } = new();
    public bool IsLiveMode { get; set; }
    public DateTime? ShowStartTime { get; set; }
    public bool JudgesVotingOpen { get; set; }
    public bool AudienceVotingOpen { get; set; }
    public Dictionary<string, object> DisplayStates { get; set; } = [];
    public DateTime? LiveCountdownTargetUtc { get; set; }
}

public class TalentShowSegmentAdvanceDTO
{
    public int SegmentId { get; set; }
}

public class TalentShowSegmentSkipDTO
{
    public int SegmentId { get; set; }
}

public class TalentShowSegmentReorderDTO
{
    public List<int> OrderedSegmentIds { get; set; } = [];
}

public class TalentShowSegmentInsertDTO
{
    public string SegmentType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int InsertAtIndex { get; set; }
    public string? MediaFile { get; set; }
    public string? HostTeleprompter { get; set; }
    public string? Backstage { get; set; }
    public int DurationSeconds { get; set; }
}

public class TalentShowJumpSegmentDTO
{
    public int SegmentId { get; set; }
}

public class TalentShowLiveStatusTransitionDTO
{
    public string TargetStatus { get; set; } = string.Empty;
    public int CountdownSeconds { get; set; }
}

public class TalentShowSegmentActionDTO
{
    public int SegmentId { get; set; }
}

public class TalentShowPerformerStatusUpdateDTO
{
    public int? ActId { get; set; }
    public string PerformerName { get; set; } = string.Empty;
    public string ActTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class TalentShowJudgeVoteDTO
{
    public int JudgeId { get; set; }
    public int ActId { get; set; }
    public int Score { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class TalentShowAudienceVoteDTO
{
    public int ActId { get; set; }
    public string? UserId { get; set; }
    public int Score { get; set; }
}

public class TalentShowVotingControlDTO
{
    public bool OpenJudgesVoting { get; set; }
    public bool OpenAudienceVoting { get; set; }
    public bool CloseVoting { get; set; }
}

public class TalentShowMediaControlDTO
{
    public string Action { get; set; } = string.Empty; // "play", "pause", "stop"
    public string MediaFile { get; set; } = string.Empty;
}

public class TalentShowCueControlDTO
{
    public string CueId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "trigger", "clear"
}

public class TalentShowTimerControlDTO
{
    public string Action { get; set; } = string.Empty; // "start", "stop", "reset", "set"
    public int Seconds { get; set; }
}

public class TalentShowShowSummaryDTO
{
    public int TotalSegmentsCompleted { get; set; }
    public int TotalActsPerformed { get; set; }
    public DateTime? ShowEndTime { get; set; }
    public int TotalDurationSeconds { get; set; }
    public int JudgesVotesCount { get; set; }
    public int AudienceVotesCount { get; set; }
    public List<TalentShowVoteResultRowDTO> JudgesResults { get; set; } = [];
    public List<TalentShowVoteResultRowDTO> AudienceResults { get; set; } = [];
    public string? ShowLogPath { get; set; }
}
