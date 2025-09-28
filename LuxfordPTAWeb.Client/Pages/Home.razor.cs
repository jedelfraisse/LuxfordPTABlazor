using LuxfordPTAWeb.Client.Services;
using LuxfordPTAWeb.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace LuxfordPTAWeb.Client.Pages;

public partial class Home : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private LuxfordPTAWeb.Client.Code.SchoolYearSupport SchoolYearSupport { get; set; } = default!;
    [Inject] private GoogleAnalyticsService GoogleAnalytics { get; set; } = default!;

    private List<AssignedUserDTO> boardMembers = new();
    private string boardYearLabel = "";
    private bool isLoadingBoard = true;
    private bool iselectionEvent = false;
    private bool isCurrentYear = false;
    private AssignedUserDTO? selectedMember = null;
    private string activeTab = "position"; // Default to position tab
    private bool hasRendered = false;

    protected override async Task OnInitializedAsync()
    {
        isLoadingBoard = true;
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
            }
        }
        catch
        {
            boardMembers = new();
        }
        isLoadingBoard = false;
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