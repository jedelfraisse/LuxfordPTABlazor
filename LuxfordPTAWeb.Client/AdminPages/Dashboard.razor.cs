using LuxfordPTAWeb.Shared.Models;
using LuxfordPTAWeb.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LuxfordPTAWeb.Client.AdminPages;

public partial class Dashboard : ComponentBase
{
    private SchoolYear? selectedSchoolYear;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            selectedSchoolYear = await SchoolYearSupport.GetSelectedSchoolYearInfoAsync();
        }
        catch (Exception)
        {
            // Handle errors silently for now
        }
    }
}
