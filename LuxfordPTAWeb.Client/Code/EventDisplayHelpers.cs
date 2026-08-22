using System.Text.Json;
using LuxfordPTAWeb.Shared.DTOs;

namespace LuxfordPTAWeb.Client.Code;

/// <summary>"When" filter option on the public /events page, shared between the page and its filter bar component.</summary>
public enum EventTimeFilter { All, Upcoming, Past }

/// <summary>
/// Shared formatting/lookup helpers for rendering EventListItemDTO on the public
/// /events page (and its Calendar/Categories tab components) so the same
/// event card looks and behaves the same no matter which view it's shown in.
/// </summary>
public static class EventDisplayHelpers
{
    public static string GetEventTimeText(EventListItemDTO evt)
    {
        var firstDay = evt.EventDays?
            .OrderBy(d => d.DayNumber)
            .FirstOrDefault();

        if (firstDay != null && !firstDay.StartTime.HasValue)
            return string.Empty; // all day event

        if (firstDay?.StartTime.HasValue == true && firstDay.EndTime.HasValue)
        {
            if (firstDay.StartTime.Value.TimeOfDay == firstDay.EndTime.Value.TimeOfDay)
            {
                return firstDay.StartTime.Value.ToString("h:mm tt");
            }

            return $"{firstDay.StartTime.Value:h:mm tt} - {firstDay.EndTime.Value:h:mm tt}";
        }

        if (evt.EventDays?.Count > 1)
        {
            return string.Empty;
        }

        if (evt.EventStartTime == default || evt.EventEndTime == default)
        {
            return string.Empty;
        }

        if (evt.EventStartTime.TimeOfDay == TimeSpan.Zero && evt.EventEndTime.TimeOfDay == TimeSpan.Zero)
        {
            return string.Empty;
        }

        if (evt.EventStartTime.TimeOfDay == evt.EventEndTime.TimeOfDay)
        {
            return evt.EventStartTime.ToString("h:mm tt");
        }

        return $"{evt.EventStartTime:h:mm tt} - {evt.EventEndTime:h:mm tt}";
    }

    public static string GetVolunteerSignupText(EventListItemDTO evt)
    {
        if (evt.SignupWindowStart.HasValue && evt.SignupWindowEnd.HasValue)
        {
            return $"Signups: {evt.SignupWindowStart.Value:MMM d h:mm tt} - {evt.SignupWindowEnd.Value:MMM d h:mm tt}";
        }

        if (evt.SignupWindowStart.HasValue)
        {
            return $"Signups open: {evt.SignupWindowStart.Value:MMM d h:mm tt}";
        }

        if (evt.SignupWindowEnd.HasValue)
        {
            return $"Signups close: {evt.SignupWindowEnd.Value:MMM d h:mm tt}";
        }

        return "Volunteers needed";
    }

    public static bool HasDetailsOrFlyer(EventListItemDTO evt)
    {
        return HasEventDetails(evt) || GetFlyerUrls(evt).Any();
    }

    public static bool HasEventDetails(EventListItemDTO evt)
    {
        return !string.IsNullOrWhiteSpace(evt.MoreDetailsMarkdown) || evt.HasPublicControlInformation;
    }

    public static string GetEventDetailsUrl(EventListItemDTO evt)
    {
        return $"/events/{evt.Slug}";
    }

    public static string GetPrimaryFlyerUrl(EventListItemDTO evt)
    {
        return GetFlyerUrls(evt).FirstOrDefault() ?? string.Empty;
    }

    public static IReadOnlyList<string> GetFlyerUrls(EventListItemDTO evt)
    {
        var flyers = new List<string>();

        if (!string.IsNullOrWhiteSpace(evt.FlyerUrl))
        {
            flyers.Add(evt.FlyerUrl.Trim());
        }

        if (!string.IsNullOrWhiteSpace(evt.FlyerUrlsJson))
        {
            try
            {
                var extraFlyers = JsonSerializer.Deserialize<List<string>>(evt.FlyerUrlsJson);
                if (extraFlyers != null)
                {
                    flyers.AddRange(extraFlyers.Where(url => !string.IsNullOrWhiteSpace(url)).Select(url => url.Trim()));
                }
            }
            catch (JsonException)
            {
            }
        }

        return flyers
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static string TruncateDescription(string description, int maxLength)
    {
        if (string.IsNullOrEmpty(description) || description.Length <= maxLength)
            return description;

        return description.Substring(0, maxLength - 3) + "...";
    }

    /// <summary>Hex color for a card's left border accent, based on the category's "text-*" color class.</summary>
    public static string GetBorderColorHex(string? colorClass)
    {
        return colorClass switch
        {
            "text-danger" => "#dc3545",
            "text-success" => "#198754",
            "text-primary" => "#0d6efd",
            "text-info" => "#0dcaf0",
            "text-warning" => "#ffc107",
            "text-secondary" => "#6c757d",
            "text-purple" => "#7F4592",
            _ => "#198754"
        };
    }

    /// <summary>Bootstrap "bg-*" class matching a category's "text-*" color class, for solid headers/badges.</summary>
    public static string GetBackgroundColorClass(string? colorClass)
    {
        return colorClass switch
        {
            "text-danger" => "bg-danger",
            "text-success" => "bg-success",
            "text-primary" => "bg-primary",
            "text-info" => "bg-info",
            "text-warning" => "bg-warning",
            "text-secondary" => "bg-secondary",
            _ => "bg-secondary"
        };
    }

    /// <summary>True if any event day falls today (used for the "Tonight!" badge).</summary>
    public static bool IsToday(EventListItemDTO evt) => evt.Date.Date == DateTime.Now.Date;
}
