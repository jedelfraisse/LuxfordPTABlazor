namespace LuxfordPTAWeb.Shared.DTOs;

public class MilestoneReorderDTO
{
    public List<MilestoneReorderItem> Items { get; set; } = [];
}

public class MilestoneReorderItem
{
    public Guid Id { get; set; }
    public int SortOrder { get; set; }
}
