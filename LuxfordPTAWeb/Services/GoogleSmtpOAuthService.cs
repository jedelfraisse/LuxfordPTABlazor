using System.Text.Json;
using LuxfordPTAWeb.Shared.Configuration;
using LuxfordPTAWeb.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LuxfordPTAWeb.Services
{
    public interface IGoogleSmtpOAuthService
    {
        /// <summary>
        /// Generate the OAuth2 authorization URL to redirect users to Google
        /// </summary>
        string GetAuthorizationUrl();

        /// <summary>
        /// Exchange authorization code for access and refresh tokens
        /// </summary>
        Task<GoogleOAuthToken> ExchangeCodeForTokenAsync(string code);

        /// <summary>
        /// Get current token (refreshing if needed)
        /// </summary>
        Task<GoogleOAuthToken?> GetValidTokenAsync();

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        Task<GoogleOAuthToken> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Check if we have valid OAuth2 credentials
        /// </summary>
        Task<bool> HasValidCredentialsAsync();

        /// <summary>
        /// Clear stored tokens (for re-authorization)
        /// </summary>
        Task ClearTokensAsync();
    }

    /// <summary>
    /// Service for managing Gmail SMTP OAuth2 authentication
    /// PTA Tech Note: This enables secure email without storing passwords
    /// </summary>
    public class GoogleSmtpOAuthService : IGoogleSmtpOAuthService
    {
        private readonly OAuth2Settings _settings;
        private readonly ILogger<GoogleSmtpOAuthService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _tokenFilePath;

        // Google OAuth2 endpoints
        private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
        private const string GmailScope = "https://mail.google.com/";

        public GoogleSmtpOAuthService(
            IOptions<OAuth2Settings> settings,
            ILogger<GoogleSmtpOAuthService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _settings = settings.Value;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();

            // Determine token storage path
            _tokenFilePath = Path.IsPathRooted(_settings.TokenStoragePath)
                ? _settings.TokenStoragePath
                : Path.Combine(AppContext.BaseDirectory, _settings.TokenStoragePath);

            // Ensure directory exists
            var directory = Path.GetDirectoryName(_tokenFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public string GetAuthorizationUrl()
        {
            var queryParams = new Dictionary<string, string>
            {
                ["client_id"] = _settings.ClientId,
                ["redirect_uri"] = _settings.RedirectUri,
                ["response_type"] = "code",
                ["scope"] = GmailScope,
                ["access_type"] = "offline", // Request refresh token
                ["prompt"] = "consent" // Force consent to ensure refresh token
            };

            var query = string.Join("&", queryParams.Select(kvp => 
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

            var authUrl = $"{AuthorizationEndpoint}?{query}";
            
            _logger.LogInformation("?? OAuth2 authorization URL generated. Redirect user to Google for consent.");
            
            return authUrl;
        }

        public async Task<GoogleOAuthToken> ExchangeCodeForTokenAsync(string code)
        {
            _logger.LogInformation("?? Exchanging authorization code for access token...");

            var requestData = new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = _settings.ClientId,
                ["client_secret"] = _settings.ClientSecret,
                ["redirect_uri"] = _settings.RedirectUri,
                ["grant_type"] = "authorization_code"
            };

            var response = await _httpClient.PostAsync(
                TokenEndpoint,
                new FormUrlEncodedContent(requestData));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("? Token exchange failed: {Error}", error);
                throw new InvalidOperationException($"OAuth2 token exchange failed: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<GoogleOAuthToken>(json);

            if (token == null)
            {
                _logger.LogError("? Failed to deserialize token response");
                throw new InvalidOperationException("Failed to parse OAuth2 token response");
            }

            token.ObtainedAt = DateTime.UtcNow;

            // Save token to file
            await SaveTokenAsync(token);

            _logger.LogInformation("? Token secured. SMTP shall flow like PTA coffee. ?");
            _logger.LogInformation("?? Access token expires at: {ExpiresAt}", token.ExpiresAt);
            
            if (string.IsNullOrEmpty(token.RefreshToken))
            {
                _logger.LogWarning("?? No refresh token received. Token will expire without automatic renewal.");
            }

            return token;
        }

        public async Task<GoogleOAuthToken?> GetValidTokenAsync()
        {
            var token = await LoadTokenAsync();

            if (token == null)
            {
                _logger.LogWarning("?? No stored token found. OAuth2 authorization required.");
                return null;
            }

            if (token.IsValid)
            {
                _logger.LogInformation("? Valid access token available (expires: {ExpiresAt})", token.ExpiresAt);
                return token;
            }

            if (token.CanRefresh)
            {
                _logger.LogInformation("?? Access token expired. Refreshing using refresh token...");
                try
                {
                    return await RefreshTokenAsync(token.RefreshToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "? Token refresh failed. Re-authorization may be required.");
                    return null;
                }
            }

            _logger.LogWarning("?? Token expired and no refresh token available. Re-authorization required.");
            return null;
        }

        public async Task<GoogleOAuthToken> RefreshTokenAsync(string refreshToken)
        {
            _logger.LogInformation("?? Refreshing access token...");

            var requestData = new Dictionary<string, string>
            {
                ["client_id"] = _settings.ClientId,
                ["client_secret"] = _settings.ClientSecret,
                ["refresh_token"] = refreshToken,
                ["grant_type"] = "refresh_token"
            };

            var response = await _httpClient.PostAsync(
                TokenEndpoint,
                new FormUrlEncodedContent(requestData));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("? Token refresh failed: {Error}", error);
                throw new InvalidOperationException($"OAuth2 token refresh failed. Did Google ghost us? ?? Error: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var newToken = JsonSerializer.Deserialize<GoogleOAuthToken>(json);

            if (newToken == null)
            {
                throw new InvalidOperationException("Failed to parse refreshed token response");
            }

            newToken.ObtainedAt = DateTime.UtcNow;
            
            // Preserve the refresh token if not included in response
            if (string.IsNullOrEmpty(newToken.RefreshToken))
            {
                newToken.RefreshToken = refreshToken;
            }

            await SaveTokenAsync(newToken);

            _logger.LogInformation("? Access token refreshed successfully. New expiry: {ExpiresAt}", newToken.ExpiresAt);

            return newToken;
        }

        public async Task<bool> HasValidCredentialsAsync()
        {
            try
            {
                var token = await GetValidTokenAsync();
                return token != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking OAuth2 credentials");
                return false;
            }
        }

        public async Task ClearTokensAsync()
        {
            if (File.Exists(_tokenFilePath))
            {
                File.Delete(_tokenFilePath);
                _logger.LogInformation("??? OAuth2 tokens cleared. Re-authorization required.");
            }
            
            await Task.CompletedTask;
        }

        private async Task SaveTokenAsync(GoogleOAuthToken token)
        {
            try
            {
                var json = JsonSerializer.Serialize(token, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                await File.WriteAllTextAsync(_tokenFilePath, json);
                
                // Set file permissions (Unix/Linux only)
                if (!OperatingSystem.IsWindows())
                {
                    File.SetUnixFileMode(_tokenFilePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
                }
                
                _logger.LogInformation("?? Token saved securely to: {Path}", _tokenFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save OAuth2 token to {Path}", _tokenFilePath);
                throw;
            }
        }

        private async Task<GoogleOAuthToken?> LoadTokenAsync()
        {
            try
            {
                if (!File.Exists(_tokenFilePath))
                {
                    return null;
                }

                var json = await File.ReadAllTextAsync(_tokenFilePath);
                var token = JsonSerializer.Deserialize<GoogleOAuthToken>(json);
                
                if (token != null)
                {
                    _logger.LogInformation("?? Token loaded from: {Path}", _tokenFilePath);
                }
                
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load OAuth2 token from {Path}", _tokenFilePath);
                return null;
            }
        }
    }
}
