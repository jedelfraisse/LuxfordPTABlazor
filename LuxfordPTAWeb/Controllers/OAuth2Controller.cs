using LuxfordPTAWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuxfordPTAWeb.Controllers
{
    /// <summary>
    /// Controller for handling Gmail OAuth2 authentication flow
    /// PTA Tech Note: This enables secure email sending without storing passwords
    /// </summary>
    [Route("oauth2")]
    public class OAuth2Controller : Controller
    {
        private readonly IGoogleSmtpOAuthService _oauthService;
        private readonly ILogger<OAuth2Controller> _logger;

        public OAuth2Controller(
            IGoogleSmtpOAuthService oauthService,
            ILogger<OAuth2Controller> logger)
        {
            _oauthService = oauthService;
            _logger = logger;
        }

        /// <summary>
        /// Initiates OAuth2 flow by redirecting to Google
        /// Admin only endpoint for security
        /// </summary>
        [HttpGet("authorize")]
        [Authorize(Roles = "Admin")]
        public IActionResult Authorize()
        {
            try
            {
                var authUrl = _oauthService.GetAuthorizationUrl();
                
                _logger.LogInformation("?? Admin initiated OAuth2 authorization flow");
                _logger.LogInformation("?? Redirecting to: {AuthUrl}", authUrl);
                
                return Redirect(authUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate OAuth2 authorization URL");
                return BadRequest(new { error = "Failed to initiate OAuth2 flow", details = ex.Message });
            }
        }

        /// <summary>
        /// OAuth2 callback endpoint - handles Google's redirect with authorization code
        /// This is the redirect URI registered in Google Cloud Console
        /// </summary>
        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string? code, [FromQuery] string? error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError("? OAuth2 authorization failed: {Error}", error);
                return Redirect($"/admin/email-oauth?error={Uri.EscapeDataString(error)}&errorDescription={Uri.EscapeDataString("Google authorization was denied or failed")}");
            }

            if (string.IsNullOrEmpty(code))
            {
                _logger.LogError("? OAuth2 callback missing authorization code");
                return Redirect("/admin/email-oauth?error=missing_code&errorDescription=Missing+authorization+code");
            }

            try
            {
                _logger.LogInformation("?? Received authorization code from Google");
                
                var token = await _oauthService.ExchangeCodeForTokenAsync(code);
                
                _logger.LogInformation("? OAuth2 handshake complete! Gmail SMTP is now authenticated.");
                _logger.LogInformation("?? Access token valid until: {ExpiresAt}", token.ExpiresAt);
                _logger.LogInformation("?? Refresh token present: {HasRefresh}", token.CanRefresh);

                // Redirect back to admin page with success message
                return Redirect("/admin/email-oauth?success=true");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? OAuth2 handshake failed. Did Google ghost us? ??");
                return Redirect($"/admin/email-oauth?error=token_exchange&errorDescription={Uri.EscapeDataString(ex.Message)}");
            }
        }

        /// <summary>
        /// Check OAuth2 status (for admin dashboard)
        /// </summary>
        [HttpGet("status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Status()
        {
            try
            {
                var hasValidCredentials = await _oauthService.HasValidCredentialsAsync();
                
                if (hasValidCredentials)
                {
                    var token = await _oauthService.GetValidTokenAsync();
                    
                    return Ok(new
                    {
                        authenticated = true,
                        expiresAt = token?.ExpiresAt,
                        hasRefreshToken = token?.CanRefresh ?? false,
                        message = "? Gmail SMTP OAuth2 is configured and working. Token secured. ?"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        authenticated = false,
                        message = "?? OAuth2 not configured or token expired. Authorization required."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking OAuth2 status");
                return StatusCode(500, new { error = "Failed to check OAuth2 status", details = ex.Message });
            }
        }

        /// <summary>
        /// Clear stored tokens and force re-authorization
        /// </summary>
        [HttpPost("revoke")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Revoke()
        {
            try
            {
                await _oauthService.ClearTokensAsync();
                
                _logger.LogInformation("??? Admin revoked OAuth2 tokens");
                
                return Ok(new { message = "OAuth2 tokens cleared. Re-authorization required." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking OAuth2 tokens");
                return StatusCode(500, new { error = "Failed to revoke tokens", details = ex.Message });
            }
        }
    }
}
