namespace LuxfordPTAWeb.Shared.Enums;

/// <summary>
/// Cookie consent levels for GDPR compliance
/// </summary>
public enum CookieConsentLevel
{
    /// <summary>
    /// No consent given - only essential cookies allowed
    /// </summary>
    Essential = 0,
    
    /// <summary>
    /// Analytics consent given - allows Google Analytics
    /// </summary>
    Analytics = 1,
    
    /// <summary>
    /// Full consent given - all cookies allowed
    /// </summary>
    All = 2
}