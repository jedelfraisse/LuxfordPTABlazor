namespace LuxfordPTAWeb.Shared.Configuration;

public class GoogleAnalyticsOptions
{
    public const string SectionName = "GoogleAnalytics";
    
    /// <summary>
    /// Google Analytics 4 Measurement ID (e.g., G-XXXXXXXXXX)
    /// </summary>
    public string MeasurementId { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether Google Analytics is enabled
    /// </summary>
    public bool Enabled { get; set; } = false;
    
    /// <summary>
    /// Whether to enable debug mode for development
    /// </summary>
    public bool Debug { get; set; } = false;
    
    /// <summary>
    /// Whether the measurement ID is configured and GA is enabled
    /// </summary>
    public bool IsConfigured => Enabled && !string.IsNullOrWhiteSpace(MeasurementId);
}