using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using LuxfordPTAWeb.Shared.Models;

namespace LuxfordPTAWeb.Client.Code;

public class SchoolYearSupport
{
    private readonly IJSRuntime _js;
    private readonly NavigationManager _navigation;
    private readonly HttpClient _http;

    public SchoolYearSupport(IJSRuntime js, NavigationManager navigation, HttpClient http)
    {
        _js = js;
        _navigation = navigation;
        _http = http;
    }

    /// <summary>
    /// Guesses "the current year" from today's date. This is only a last-resort fallback for when
    /// no school year has been marked Status=CurrentYear yet (or the API call failed) — the real
    /// source of truth is <see cref="GetActualCurrentSchoolYearAsync"/>, which an admin controls
    /// explicitly rather than it flipping automatically at a calendar boundary.
    /// </summary>
    public (int StartYear, int EndYear) GetCurrentSchoolYear()
    {
        var today = DateTime.Today;
        int year = today.Month >= 7 ? today.Year : today.Year - 1;
        return (year, year + 1);
    }

    /// <summary>The school year with Status=CurrentYear, as the server sees it (respects visibility for anonymous callers). Null if none is configured/visible.</summary>
    public async Task<SchoolYear?> GetActualCurrentSchoolYearAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<SchoolYear>("api/schoolyears/current");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>The start year of the Status=CurrentYear school year, falling back to a date-based guess if none is configured.</summary>
    public async Task<int> GetActualCurrentYearAsync()
    {
        var current = await GetActualCurrentSchoolYearAsync();
        return current?.StartDate.Year ?? GetCurrentSchoolYear().StartYear;
    }

    public async Task<bool> IsLocalStorageAvailableAsync()
    {
        try
        {
            // Try a simple set/get/remove to verify localStorage access
            await _js.InvokeVoidAsync("localStorage.setItem", "__test", "1");
            var testValue = await _js.InvokeAsync<string>("localStorage.getItem", "__test");
            await _js.InvokeVoidAsync("localStorage.removeItem", "__test");
            return testValue == "1";
        }
        catch
        {
            return false;
        }
    }

    public async Task<int> GetSelectedSchoolYearAsync()
    {
        if (!await IsLocalStorageAvailableAsync())
            return await GetActualCurrentYearAsync();

        var storedYear = await _js.InvokeAsync<string>("localStorage.getItem", "selectedSchoolYear");
        if (int.TryParse(storedYear, out var year))
            return year;
        return await GetActualCurrentYearAsync();
    }

    public async Task SetSelectedSchoolYearAsync(int year)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", "selectedSchoolYear", year.ToString());
    }

    /// <summary>
    /// Gets the full SchoolYear info for the selected year (from local storage or current year).
    /// </summary>
    public async Task<SchoolYear?> GetSelectedSchoolYearInfoAsync()
    {
        var selectedYear = await GetSelectedSchoolYearAsync();
        var schoolYears = await _http.GetFromJsonAsync<List<SchoolYear>>("api/schoolyears");
        if (schoolYears == null)
            return null;

        return schoolYears.FirstOrDefault(sy => sy.StartDate.Year == selectedYear);
    }

    public void ReloadPage()
    {
        _navigation.NavigateTo(_navigation.Uri, forceLoad: true);
    }

    public async Task<bool> CanChangeSchoolYearAsync()
    {
        return await IsLocalStorageAvailableAsync();
    }
}
