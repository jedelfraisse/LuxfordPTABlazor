using System;

namespace LuxfordPTAWeb.Shared.DTOs;

public class EventListDayDTO
{
    public int Id { get; set; }
    public int DayNumber { get; set; }
    public DateTime Date { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}
