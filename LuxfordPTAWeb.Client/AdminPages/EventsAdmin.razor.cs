using LuxfordPTAWeb.Shared.Models;
using LuxfordPTAWeb.Shared.DTOs;
using LuxfordPTAWeb.Shared.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace LuxfordPTAWeb.Client.AdminPages;

public partial class EventsAdmin : ComponentBase
{
    private List<Event>? allEvents;
    private List<Event>? filteredEvents;
    private List<EventCat>? eventCats;
    private List<EventCatSub>? allEventSubCats;
    private List<EventCatSub>? availableSubCats;
    private List<SchoolYear>? schoolYears;
    private SchoolYear? selectedSchoolYear;
    private EventDashboardSummaryDTO? eventSummary;
    private int selectedEventCatId = 0;
    private int selectedEventSubCatId = 0;
    private int selectedSchoolYearId = 0;
    private string selectedQuickFilter = "all";

    private string GetCreateEventUrl()
    {
        var url = "/admin/events/create";
        var queryParams = new List<string>();
        if (selectedSchoolYearId > 0)
            queryParams.Add($"schoolYearId={selectedSchoolYearId}");
        if (selectedEventCatId > 0)
            queryParams.Add($"eventCatId={selectedEventCatId}");
        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);
        return url;
    }

    protected override async Task OnInitializedAsync()
    {
        // Check for query parameters
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);
        if (query.TryGetValue("filter", out var filterValue))
        {
            selectedQuickFilter = filterValue.ToString().ToLower();
        }

        await LoadSchoolYears();
        await LoadEventCats();
        await LoadEventSubCats();
        await LoadEvents();
        await LoadEventSummary();
    }

    private async Task LoadEvents()
    {
        try
        {
            // Get events with optional school year filtering
            var url = selectedSchoolYearId > 0
                ? $"api/events/all-admin?schoolYearId={selectedSchoolYearId}"
                : "api/events/all-admin";

            allEvents = await Http.GetFromJsonAsync<List<Event>>(url);
            if (allEvents == null)
            {
                allEvents = new List<Event>();
            }
            ApplyFilters();
        }
        catch (Exception)
        {
            allEvents = new List<Event>();
            filteredEvents = new List<Event>();
        }
    }

    private async Task LoadEventSummary()
    {
        try
        {
            var url = selectedSchoolYearId > 0
                ? $"api/events/dashboard-summary/{selectedSchoolYearId}"
                : "api/events/dashboard-summary";

            eventSummary = await Http.GetFromJsonAsync<EventDashboardSummaryDTO>(url);
        }
        catch (Exception)
        {
            // Handle errors silently
        }
    }

    private async Task LoadEventCats()
    {
        try
        {
            eventCats = await Http.GetFromJsonAsync<List<EventCat>>("api/eventcat");
        }
        catch (Exception)
        {
            eventCats = new List<EventCat>();
        }
    }

    private async Task LoadEventSubCats()
    {
        try
        {
            allEventSubCats = await Http.GetFromJsonAsync<List<EventCatSub>>("api/eventcatsub");
            UpdateAvailableSubCats();
        }
        catch (Exception)
        {
            allEventSubCats = new List<EventCatSub>();
            availableSubCats = new List<EventCatSub>();
        }
    }

    private void UpdateAvailableSubCats()
    {
        if (allEventSubCats == null)
        {
            availableSubCats = new List<EventCatSub>();
            return;
        }

        if (selectedEventCatId > 0)
        {
            availableSubCats = allEventSubCats
                .Where(sc => sc.EventCatId == selectedEventCatId && sc.IsActive)
                .OrderBy(sc => sc.DisplayOrder)
                .ThenBy(sc => sc.Name)
                .ToList();
        }
        else
        {
            availableSubCats = new List<EventCatSub>();
        }
    }

    private async Task LoadSchoolYears()
    {
        try
        {
            schoolYears = await Http.GetFromJsonAsync<List<SchoolYear>>("api/schoolyears");
        }
        catch (Exception)
        {
            schoolYears = new List<SchoolYear>();
        }
    }

    private async Task SetQuickFilter(string filter)
    {
        selectedQuickFilter = filter;

        // Auto-select current school year when applying quick filters
        if (filter != "all" && selectedSchoolYearId == 0)
        {
            var currentYear = schoolYears?.FirstOrDefault(sy => sy.Status == LuxfordPTAWeb.Shared.Enums.SchoolYearStatus.CurrentYear);
            if (currentYear != null)
            {
                selectedSchoolYearId = currentYear.Id;
                selectedSchoolYear = currentYear;
                await LoadEvents(); // Reload events with new school year filter
                await LoadEventSummary(); // Reload summary with new filter
            }
        }

        ApplyFilters();
    }

    private async Task FilterBySchoolYear(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int schoolYearId))
        {
            selectedSchoolYearId = schoolYearId;
            selectedSchoolYear = schoolYears?.FirstOrDefault(sy => sy.Id == schoolYearId);
            await LoadEvents(); // Reload events with new filter
            await LoadEventSummary(); // Reload summary with new filter
            ApplyFilters();
        }
    }

    private async Task FilterByEventCat(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int eventCatId))
        {
            selectedEventCatId = eventCatId;
            selectedEventSubCatId = 0; // Reset subcategory selection
            UpdateAvailableSubCats();
            ApplyFilters();
        }
    }

    private async Task FilterByEventSubCat(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int eventSubCatId))
        {
            selectedEventSubCatId = eventSubCatId;
            ApplyFilters();
        }
    }

    private void ApplyFilters()
    {
        if (allEvents == null) return;

        IEnumerable<Event> filtered = allEvents;

        // Apply quick filters first
        var now = DateTime.UtcNow;
        var next30Days = now.AddDays(30);

        filtered = selectedQuickFilter switch
        {
            "pending" => filtered.Where(e => e.Status == EventStatus.SubmittedForApproval),
            "volunteers" => filtered.Where(e => e.RequiresVolunteers && e.Date >= now &&
                (e.Status == EventStatus.Active || e.Status == EventStatus.InProgress)),
            "upcoming" => filtered.Where(e => e.Date >= now && e.Date <= next30Days &&
                (e.Status == EventStatus.Active || e.Status == EventStatus.InProgress)),
            "active" => filtered.Where(e => e.Status == EventStatus.Active),
            "planning" => filtered.Where(e => e.Status == EventStatus.Planning),
            _ => filtered
        };

        if (selectedSchoolYearId > 0)
        {
            filtered = filtered.Where(e => e.SchoolYear?.Id == selectedSchoolYearId);
        }

        if (selectedEventCatId > 0)
        {
            filtered = filtered.Where(e => e.EventCat?.Id == selectedEventCatId);
        }

        if (selectedEventSubCatId > 0)
        {
            filtered = filtered.Where(e => e.EventCatSub?.Id == selectedEventSubCatId);
        }

        filteredEvents = filtered.ToList();
    }

    private async Task ClearFilters()
    {
        selectedSchoolYearId = 0;
        selectedEventCatId = 0;
        selectedEventSubCatId = 0;
        selectedSchoolYear = null;
        selectedQuickFilter = "all";
        availableSubCats = new List<EventCatSub>();

        // Reload data when clearing filters
        await LoadEvents();
        await LoadEventSummary();
        ApplyFilters();
    }

    private async Task ShowCurrentYear()
    {
        var currentYear = schoolYears?.FirstOrDefault(sy => sy.Status == LuxfordPTAWeb.Shared.Enums.SchoolYearStatus.CurrentYear);
        if (currentYear != null)
        {
            selectedSchoolYearId = currentYear.Id;
            selectedSchoolYear = currentYear;
        }
        selectedEventCatId = 0;
        selectedEventSubCatId = 0;
        selectedQuickFilter = "all";
        availableSubCats = new List<EventCatSub>();

        // Reload data when showing current year
        await LoadEvents();
        await LoadEventSummary();
        ApplyFilters();
    }

    private string GetFilterDescription()
    {
        var parts = new List<string>();

        if (selectedQuickFilter != "all")
        {
            parts.Add($"with filter '{selectedQuickFilter}'");
        }

        if (selectedSchoolYearId > 0)
            parts.Add($"in {selectedSchoolYear?.Name}");

        if (selectedEventCatId > 0)
        {
            var eventCat = eventCats?.FirstOrDefault(ec => ec.Id == selectedEventCatId);
            parts.Add($"for {eventCat?.Name}");

            if (selectedEventSubCatId > 0)
            {
                var eventSubCat = allEventSubCats?.FirstOrDefault(esc => esc.Id == selectedEventSubCatId);
                parts.Add($"in {eventSubCat?.Name}");
            }
        }

        return parts.Any() ? string.Join(" ", parts) : "";
    }

    private string GetStatusBadgeClass(EventStatus status) => status switch
    {
        EventStatus.Planning => "bg-secondary",
        EventStatus.SubmittedForApproval => "bg-warning",
        EventStatus.Active => "bg-success",
        EventStatus.InProgress => "bg-primary",
        EventStatus.WrapUp => "bg-info",
        EventStatus.Completed => "bg-dark",
        EventStatus.Cancelled => "bg-danger",
        _ => "bg-secondary"
    };

    private async Task ApproveEvent(int eventId)
    {
        if (await ConfirmApproval())
        {
            try
            {
                HttpResponseMessage response = await Http.PostAsync($"api/events/{eventId}/approve", null);
                if (response.IsSuccessStatusCode)
                {
                    await LoadEvents();
                    await LoadEventSummary();
                }
            }
            catch (Exception)
            {
                // Handle errors
            }
        }
    }

    private async Task DeleteEvent(int eventId)
    {
        if (await ConfirmDelete())
        {
            try
            {
                HttpResponseMessage response = await Http.DeleteAsync($"api/events/{eventId}");
                if (response.IsSuccessStatusCode)
                {
                    await LoadEvents();
                    await LoadEventSummary();
                }
            }
            catch (Exception)
            {
                // Handle errors
            }
        }
    }

    private async Task<bool> ConfirmDelete()
    {
        return await JS.InvokeAsync<bool>("confirm", "Are you sure you want to delete this event?");
    }

    private async Task<bool> ConfirmApproval()
    {
        return await JS.InvokeAsync<bool>("confirm", "Are you sure you want to approve this event?");
    }
}