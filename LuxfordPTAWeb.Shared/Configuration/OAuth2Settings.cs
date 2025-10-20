namespace LuxfordPTAWeb.Shared.Configuration
{
    /// <summary>
    /// Configuration for Gmail SMTP OAuth2 authentication
    /// PTA Tech Note: These settings enable secure email sending without storing passwords
    /// </summary>
    public class OAuth2Settings
    {
        public const string SectionName = "OAuth2Settings";

        /// <summary>
        /// Google OAuth2 Client ID from Google Cloud Console
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Google OAuth2 Client Secret from Google Cloud Console
        /// SECURITY: Store this in user secrets for dev, secure config for production
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// OAuth2 redirect URI - must match what's configured in Google Cloud Console
        /// Dev: https://localhost:7123/oauth2callback
        /// Prod: https://luxfordpta.org/oauth2callback
        /// </summary>
        public string RedirectUri { get; set; } = string.Empty;

        /// <summary>
        /// Path to store the OAuth2 tokens securely
        /// </summary>
        public string TokenStoragePath { get; set; } = "Data/gmail-token.json";

        /// <summary>
        /// Email address that will be used for sending (must have Gmail API access)
        /// </summary>
        public string EmailAddress { get; set; } = "maildragon@luxfordpta.org";

        /// <summary>
        /// Whether to enable OAuth2 for Gmail SMTP (falls back to password auth if false)
        /// </summary>
        public bool EnableOAuth2 { get; set; } = false;
    }
}
