using Microsoft.JSInterop;
using LuxfordPTAWeb.Shared.Configuration;
using LuxfordPTAWeb.Shared.Services;

namespace LuxfordPTAWeb.Client.Services;

/// <summary>
/// Service for managing Google Analytics 4 integration with consent management
/// </summary>
public class GoogleAnalyticsService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ICookieConsentService _cookieConsentService;
    private readonly GoogleAnalyticsOptions _options;
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    private bool _isInitialized;
    
    public GoogleAnalyticsService(
        IJSRuntime jsRuntime, 
        ICookieConsentService cookieConsentService,
        GoogleAnalyticsOptions options)
    {
        _jsRuntime = jsRuntime;
        _cookieConsentService = cookieConsentService;
        _options = options;
        _moduleTask = new(() => _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/LuxfordPTAWeb.Client/js/googleAnalytics.js").AsTask());
        
        // Subscribe to consent changes
        _cookieConsentService.ConsentChanged += OnConsentChanged;
    }
    
    /// <summary>
    /// Check if JavaScript interop is available (not during prerendering)
    /// </summary>
    private bool IsJSAvailable => _jsRuntime is IJSInProcessRuntime;
    
    /// <summary>
    /// Initialize Google Analytics if consent is given and configuration is valid
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized || !_options.IsConfigured || !IsJSAvailable)
            return;
            
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("initializeGA", _options.MeasurementId, _options.Debug);
            _isInitialized = true;
            
            // Check if we already have analytics consent and grant it
            var hasConsent = await _cookieConsentService.IsAnalyticsAllowedAsync();
            if (hasConsent)
            {
                await module.InvokeVoidAsync("grantAnalyticsConsent");
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw - analytics is not critical
            Console.WriteLine($"Failed to initialize Google Analytics: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Track a page view
    /// </summary>
    public async Task TrackPageViewAsync(string pageName, string pageTitle = "")
    {
        if (!_isInitialized || !_options.IsConfigured || !IsJSAvailable)
            return;
            
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("trackPageView", pageName, pageTitle);
        }
        catch
        {
            // Silently fail - analytics is not critical
        }
    }
    
    /// <summary>
    /// Track a custom event
    /// </summary>
    public async Task TrackEventAsync(string eventName, object? eventData = null)
    {
        if (!_isInitialized || !_options.IsConfigured || !IsJSAvailable)
            return;
            
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("trackEvent", eventName, eventData);
        }
        catch
        {
            // Silently fail - analytics is not critical
        }
    }
    
    /// <summary>
    /// Set user ID for analytics (when user is logged in)
    /// </summary>
    public async Task SetUserIdAsync(string userId)
    {
        if (!_isInitialized || !_options.IsConfigured || !IsJSAvailable)
            return;
            
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("setUserId", userId);
        }
        catch
        {
            // Silently fail - analytics is not critical
        }
    }
    
    /// <summary>
    /// Grant analytics consent
    /// </summary>
    private async Task GrantConsentAsync()
    {
        if (!_isInitialized || !IsJSAvailable) return;
        
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("grantAnalyticsConsent");
        }
        catch
        {
            // Silently fail
        }
    }
    
    /// <summary>
    /// Revoke analytics consent
    /// </summary>
    private async Task RevokeConsentAsync()
    {
        if (!_isInitialized || !IsJSAvailable) return;
        
        try
        {
            var module = await _moduleTask.Value;
            await module.InvokeVoidAsync("revokeAnalyticsConsent");
        }
        catch
        {
            // Silently fail
        }
    }
    
    private async void OnConsentChanged(LuxfordPTAWeb.Shared.Enums.CookieConsentLevel level)
    {
        if (!IsJSAvailable) return;
        
        // Initialize GA if not already done
        if (!_isInitialized)
        {
            await InitializeAsync();
        }
        
        // Update consent based on level
        if (level >= LuxfordPTAWeb.Shared.Enums.CookieConsentLevel.Analytics)
        {
            await GrantConsentAsync();
        }
        else
        {
            await RevokeConsentAsync();
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        _cookieConsentService.ConsentChanged -= OnConsentChanged;
        
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