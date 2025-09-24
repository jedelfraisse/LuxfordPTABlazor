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
    private bool _isInitialized;
    
    public GoogleAnalyticsService(
        IJSRuntime jsRuntime, 
        ICookieConsentService cookieConsentService,
        GoogleAnalyticsOptions options)
    {
        _jsRuntime = jsRuntime;
        _cookieConsentService = cookieConsentService;
        _options = options;
        
        // Subscribe to consent changes
        _cookieConsentService.ConsentChanged += OnConsentChanged;
    }
    
    /// <summary>
    /// Check if JavaScript interop is available (not during prerendering)
    /// </summary>
    private bool IsJSAvailable => _jsRuntime is IJSInProcessRuntime;
    
    /// <summary>
    /// Initialize Google Analytics consent management (gtag is already loaded in HTML)
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized || !_options.IsConfigured || !IsJSAvailable)
            return;
            
        try
        {
            // Check if gtag is available (loaded from HTML head)
            var hasGtag = await _jsRuntime.InvokeAsync<bool>("eval", "typeof window.gtag !== 'undefined'");
            
            if (!hasGtag)
            {
                Console.WriteLine("Google Analytics gtag not found - may not be loaded yet");
                return;
            }
            
            _isInitialized = true;
            
            // Check current consent level and apply it
            var currentLevel = await _cookieConsentService.GetConsentLevelAsync();
            if (currentLevel >= LuxfordPTAWeb.Shared.Enums.CookieConsentLevel.Analytics)
            {
                // Already granted by default in HTML, no action needed
                Console.WriteLine("Google Analytics consent already granted");
            }
            else
            {
                // User has opted out, revoke consent
                await RevokeConsentAsync();
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw - analytics is not critical
            Console.WriteLine($"Failed to initialize Google Analytics consent: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Track a page view using the global gtag function
    /// </summary>
    public async Task TrackPageViewAsync(string pageName, string pageTitle = "")
    {
        if (!_isInitialized || !_options.IsConfigured || !IsJSAvailable)
            return;
            
        try
        {
            await _jsRuntime.InvokeVoidAsync("gtag", "event", "page_view", new
            {
                page_title = pageTitle ?? pageName,
                page_location = await _jsRuntime.InvokeAsync<string>("eval", "window.location.href"),
                page_path = pageName
            });
        }
        catch
        {
            // Silently fail - analytics is not critical
        }
    }
    
    /// <summary>
    /// Track a custom event using the global gtag function
    /// </summary>
    public async Task TrackEventAsync(string eventName, object? eventData = null)
    {
        if (!_isInitialized || !_options.IsConfigured || !IsJSAvailable)
            return;
            
        try
        {
            if (eventData != null)
            {
                await _jsRuntime.InvokeVoidAsync("gtag", "event", eventName, eventData);
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("gtag", "event", eventName);
            }
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
            await _jsRuntime.InvokeVoidAsync("gtag", "config", _options.MeasurementId, new
            {
                user_id = userId
            });
        }
        catch
        {
            // Silently fail - analytics is not critical
        }
    }
    
    /// <summary>
    /// Grant analytics consent using global gtag
    /// </summary>
    private async Task GrantConsentAsync()
    {
        if (!_isInitialized || !IsJSAvailable) return;
        
        try
        {
            await _jsRuntime.InvokeVoidAsync("gtag", "consent", "update", new
            {
                analytics_storage = "granted"
            });
            Console.WriteLine("Analytics consent granted");
        }
        catch
        {
            // Silently fail
        }
    }
    
    /// <summary>
    /// Revoke analytics consent using global gtag
    /// </summary>
    private async Task RevokeConsentAsync()
    {
        if (!_isInitialized || !IsJSAvailable) return;
        
        try
        {
            await _jsRuntime.InvokeVoidAsync("gtag", "consent", "update", new
            {
                analytics_storage = "denied"
            });
            Console.WriteLine("Analytics consent revoked");
        }
        catch
        {
            // Silently fail
        }
    }
    
    private async void OnConsentChanged(LuxfordPTAWeb.Shared.Enums.CookieConsentLevel level)
    {
        if (!IsJSAvailable) return;
        
        // Initialize if not already done
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
        // No JavaScript module to dispose since we're using global gtag
    }
}