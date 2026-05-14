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
    public List<string> SessionLabels { get; set; } = [];
    public List<string> AssignedHelpers { get; set; } = [];
    public int SlotLengthMinutes { get; set; } = 5;
    public int BreakMinutes { get; set; }
    public bool AutoSchedule { get; set; }
    public int MaxPerformersPerSession { get; set; } = 30;
    public bool RequireMedia { get; set; }
    public bool RequireGuardian { get; set; }
    public bool RequireEquipmentList { get; set; }
    public bool RequireCategory { get; set; }
    public int ScoringScaleMax { get; set; } = 5;
    public List<TalentShowTryOutScoringFieldDTO> ScoringFields { get; set; } = [];
    public bool EnableCommentBox { get; set; } = true;
}

public class TalentShowTryOutScoringFieldDTO
{
    public string FieldId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int MaxScore { get; set; } = 5;
    public int OrderIndex { get; set; }
}

public class TalentShowTryOutEntryUpsertDTO
{
    public int? SignupId { get; set; }
    public string PerformerNames { get; set; } = string.Empty;
    public string ActTitle { get; set; } = string.Empty;
    public DateTime? SlotTime { get; set; }
    public string SessionLabel { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool Selected { get; set; }
}
