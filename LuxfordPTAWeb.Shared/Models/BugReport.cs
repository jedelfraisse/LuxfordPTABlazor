using System;

namespace LuxfordPTAWeb.Shared.Models
{
    public class BugReport
    {
        public int Id { get; set; }
        public string PageName { get; set; } = string.Empty;
        public string PageRoute { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Suggestion { get; set; } = string.Empty;
        public string SubmittedBy { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public bool IsResolved { get; set; }
    }
}
