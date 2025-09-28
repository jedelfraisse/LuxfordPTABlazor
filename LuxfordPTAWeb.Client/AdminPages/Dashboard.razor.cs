using LuxfordPTAWeb.Shared.Models;
using LuxfordPTAWeb.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace LuxfordPTAWeb.Client.AdminPages;

public partial class Dashboard : ComponentBase
{
    private SchoolYear? selectedSchoolYear;
    private List<Event>? filteredEvents;
    private EventDashboardSummaryDTO? eventSummary;
    private List<ApplicationUser> availableUsers = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            selectedSchoolYear = await SchoolYearSupport.GetSelectedSchoolYearInfoAsync();

            if (selectedSchoolYear != null)
            {
                // Load all events for the selected school year
                var events = await Http.GetFromJsonAsync<List<Event>>($"api/events/all-admin?schoolYearId={selectedSchoolYear.Id}");
                filteredEvents = events ?? new List<Event>();

                // Load event summary
                await LoadEventSummaryAsync();
            }
            
            // Load available users
            availableUsers = (await Http.GetFromJsonAsync<List<ApplicationUser>>("api/users") ?? new List<ApplicationUser>())
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToList();
        }
        catch (Exception)
        {
            // Handle errors silently for now
        }
    }

    private async Task LoadEventSummaryAsync()
    {
        try
        {
            if (selectedSchoolYear == null) return;
            eventSummary = await Http.GetFromJsonAsync<EventDashboardSummaryDTO>($"api/events/dashboard-summary/{selectedSchoolYear.Id}");
        }
        catch (Exception)
        {
            // Handle errors silently for now
        }
    }
}