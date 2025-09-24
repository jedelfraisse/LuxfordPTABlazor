using LuxfordPTAWeb.Shared.Enums;

namespace LuxfordPTAWeb.Shared.Services;

/// <summary>
/// Service for managing cookie consent for GDPR compliance
/// </summary>
public interface ICookieConsentService
{
    /// <summary>
    /// Event fired when consent level changes
    /// </summary>
    event Action<CookieConsentLevel>? ConsentChanged;
    
    /// <summary>
    /// Get the current consent level
    /// </summary>
    Task<CookieConsentLevel> GetConsentLevelAsync();
    
    /// <summary>
    /// Set the consent level
    /// </summary>
    Task SetConsentLevelAsync(CookieConsentLevel level);
    
    /// <summary>
    /// Check if analytics is allowed
    /// </summary>
    Task<bool> IsAnalyticsAllowedAsync();
    
    /// <summary>
    /// Check if consent has been given (any level above Essential)
    /// </summary>
    Task<bool> HasConsentBeenGivenAsync();
    
    /// <summary>
    /// Reset all consent preferences
    /// </summary>
    Task ResetConsentAsync();
}