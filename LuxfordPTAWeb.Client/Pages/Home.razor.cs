using LuxfordPTAWeb.Client.Services;
using LuxfordPTAWeb.Shared.DTOs;
using LuxfordPTAWeb.Shared.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace LuxfordPTAWeb.Client.Pages;

public partial class Home : ComponentBase
{
    private List<AssignedUserDTO> boardMembers = new();
    private string boardYearLabel = "";
    private bool isLoadingBoard = true;
    private bool iselectionEvent = false;
    private bool isCurrentYear = false;
    private AssignedUserDTO? selectedMember = null;
    private string activeTab = "position"; // Default to position tab
    private bool hasRendered = false;
    
    // Event lists for home page
    private List<Event> previousEvents = new();
    private List<Event> upcomingEvents = new();
    private bool isLoadingEvents = true;

    protected override async Task OnInitializedAsync()
    {
        isLoadingBoard = true;
        isLoadingEvents = true;
        
        try
        {
            // Get the current school year
            var schoolYear = await SchoolYearSupport.GetSelectedSchoolYearInfoAsync();
            if (schoolYear != null)
            {
                isCurrentYear = schoolYear.Status == Shared.Enums.SchoolYearStatus.CurrentYear;
                boardYearLabel = $"{schoolYear.Name}";

                // Use the new public endpoint that returns AssignedUserDTO
                boardMembers = await Http.GetFromJsonAsync<List<AssignedUserDTO>>($"api/boardpositions/public/by-schoolyear/{schoolYear.Id}") 
                    ?? new List<AssignedUserDTO>();
                    
                // Load events if this is the current year
                if (isCurrentYear)
                {
                    await LoadEventsAsync(schoolYear.Id);
                }
            }
        }
        catch
        {
            boardMembers = new();
            previousEvents = new();
            upcomingEvents = new();
        }
        
        isLoadingBoard = false;
        isLoadingEvents = false;
    }
    
    private async Task LoadEventsAsync(int schoolYearId)
    {
        try
        {
            // Get all events for the school year
            var allEvents = await Http.GetFromJsonAsync<List<Event>>($"api/events/by-school-year/{schoolYearId}");
            
            if (allEvents != null && allEvents.Any())
            {
                var now = DateTime.UtcNow.Date;
                
                // Get the 5 most recent previous events (ordered by date descending)
                previousEvents = allEvents
                    .Where(e => e.Date < now)
                    .OrderByDescending(e => e.Date)
                    .Take(5)
                    .ToList();
                
                // Get the next 5 upcoming events (ordered by date ascending)
                upcomingEvents = allEvents
                    .Where(e => e.Date >= now)
                    .OrderBy(e => e.Date)
                    .Take(5)
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            // Log error if needed
            previousEvents = new();
            upcomingEvents = new();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !hasRendered)
        {
            hasRendered = true;
            // Track page view only after rendering is complete
            await GoogleAnalytics.TrackPageViewAsync("/", "Home - Luxford Elementary PTA");
        }
    }

    private async Task ShowMemberDetails(AssignedUserDTO member)
    {
        if (member.HasAdditionalDetails)
        {
            selectedMember = member;

            // Track board member modal view (safe to call after render)
            if (hasRendered)
            {
                await GoogleAnalytics.TrackEventAsync("board_member_details", new 
                { 
                    member_name = member.DisplayName,
                    member_role = member.RoleTitle,
                    page = "home"
                });
            }

            // Set default active tab based on available content
            bool hasPositionDescription = !string.IsNullOrWhiteSpace(member.RoleDescription);
            bool hasBio = !string.IsNullOrWhiteSpace(member.Bio);

            if (hasPositionDescription)
            {
                activeTab = "position";
            }
            else if (hasBio)
            {
                activeTab = "bio";
            }

            StateHasChanged();
        }
    }

    private void CloseModal()
    {
        selectedMember = null;
        activeTab = "position";
        StateHasChanged();
    }

    private void SetActiveTab(string tab)
    {
        activeTab = tab;
        StateHasChanged();
    }
}