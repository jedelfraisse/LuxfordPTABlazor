using Microsoft.JSInterop;
using LuxfordPTAWeb.Shared.Enums;
using LuxfordPTAWeb.Shared.Services;

namespace LuxfordPTAWeb.Client.Services;

/// <summary>
/// Client-side implementation of cookie consent service using local storage
/// </summary>
public class CookieConsentService : ICookieConsentService, IAsyncDisposable
{
    private const string CONSENT_KEY = "luxford-pta-cookie-consent";
    private const int CONSENT_EXPIRY_DAYS = 365;
    
    private readonly IJSRuntime _jsRuntime;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    private CookieConsentLevel? _cachedConsent;
    
    public event Action<CookieConsentLevel>? ConsentChanged;
    
    public CookieConsentService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _moduleTask = new(() => _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/preferences.js").AsTask());
    }
    
    /// <summary>
    /// Check if JavaScript interop is available (not during prerendering)
    /// </summary>
    private bool IsJSAvailable => _jsRuntime is IJSInProcessRuntime;
    
    public async Task<CookieConsentLevel> GetConsentLevelAsync()
    {
        if (_cachedConsent.HasValue)
            return _cachedConsent.Value;
        
        if (!IsJSAvailable)
        {
            // Default to Analytics level (opt-out approach) during prerendering
            _cachedConsent = CookieConsentLevel.Analytics;
            return CookieConsentLevel.Analytics;
        }
            
        try
        {
            var module = await _moduleTask.Value;
            var consentValue = await module.InvokeAsync<string>("getConsent", CONSENT_KEY);
            
            if (string.IsNullOrEmpty(consentValue) || !Enum.TryParse<CookieConsentLevel>(consentValue, out var level))
            {
                // No consent given yet - default to Analytics level (opt-out approach)
                // This means Google Analytics will be enabled by default until user opts out
                _cachedConsent = CookieConsentLevel.Analytics;
                return CookieConsentLevel.Analytics;
            }
            
            _cachedConsent = level;
            return level;
        }
        catch
        {
            // Fall back to Analytics if there's any issue (opt-out approach)
            _cachedConsent = CookieConsentLevel.Analytics;
            return CookieConsentLevel.Analytics;
        }
    }
    
    public async Task SetConsentLevelAsync(CookieConsentLevel level)
    {
        if (!IsJSAvailable)
        {
            _cachedConsent = level;
            return;
        }
        
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("setConsent", CONSENT_KEY, level.ToString(), CONSENT_EXPIRY_DAYS);
            _cachedConsent = level;
            ConsentChanged?.Invoke(level);
        }
        catch
        {
            // Silently fail - consent will remain at default level
        }
    }
    
    public async Task<bool> IsAnalyticsAllowedAsync()
    {
        var level = await GetConsentLevelAsync();
        return level >= CookieConsentLevel.Analytics;
    }
    
    public async Task<bool> HasConsentBeenGivenAsync()
    {
        if (!IsJSAvailable)
            return false;
            
        try
        {
            var module = await _moduleTask.Value;
            var consentValue = await module.InvokeAsync<string>("getConsent", CONSENT_KEY);
            return !string.IsNullOrEmpty(consentValue);
        }
        catch
        {
            return false;
        }
    }
    
    public async Task ResetConsentAsync()
    {
        if (!IsJSAvailable)
        {
            // Reset to default Analytics level (opt-out approach)
            _cachedConsent = CookieConsentLevel.Analytics;
            return;
        }
        
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("removeConsent", CONSENT_KEY);
            // Reset to default Analytics level instead of Essential
            _cachedConsent = CookieConsentLevel.Analytics;
            ConsentChanged?.Invoke(CookieConsentLevel.Analytics);
        }
        catch
        {
            // Silently fail
        }
    }
    
    private async Task<string?> GetConsentDirectAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", CONSENT_KEY);
        }
        catch
        {
            return null;
        }
    }

    private async Task SetConsentDirectAsync(string value)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CONSENT_KEY, value);
            
            // Also set expiry
            var expiryDate = DateTime.Now.AddDays(CONSENT_EXPIRY_DAYS);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CONSENT_KEY + "_expiry", expiryDate.ToString("O"));
        }
        catch
        {
            // Silently fail
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        // Only dispose JS module if JS runtime is available
        if (_moduleTask.IsValueCreated && IsJSAvailable)
        {
            try
            {
                var module = await _moduleTask.Value;
                await module.DisposeAsync();
            }
            catch
            {
                // Silently fail during disposal - service might be disposing during prerendering
            }
        }
    }
}