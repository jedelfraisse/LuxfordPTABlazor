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
