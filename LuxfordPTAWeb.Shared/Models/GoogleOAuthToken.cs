using System.Text.Json.Serialization;

namespace LuxfordPTAWeb.Shared.Models
{
    /// <summary>
    /// Represents OAuth2 tokens for Gmail SMTP access
    /// </summary>
    public class GoogleOAuthToken
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = "Bearer";

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;

        /// <summary>
        /// When the token was obtained (calculated locally)
        /// </summary>
        public DateTime ObtainedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the access token will expire (calculated locally)
        /// </summary>
        public DateTime ExpiresAt => ObtainedAt.AddSeconds(ExpiresIn);

        /// <summary>
        /// Whether the access token is currently valid
        /// </summary>
        public bool IsValid => DateTime.UtcNow < ExpiresAt.AddMinutes(-5); // 5 min buffer

        /// <summary>
        /// Whether we have a refresh token to get new access tokens
        /// </summary>
        public bool CanRefresh => !string.IsNullOrEmpty(RefreshToken);
    }
}
