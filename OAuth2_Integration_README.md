# Gmail OAuth2 SMTP Integration

## Overview

The Luxford PTA Web application now supports **OAuth2 authentication for Gmail SMTP**, providing secure email sending without storing passwords in configuration files.

### Key Features

- ?? **Passwordless Authentication** - No credentials in config files
- ?? **Automatic Token Refresh** - Tokens refresh automatically before expiry
- ? **Coffee-Powered Diagnostics** - Cheeky log messages for easy monitoring
- ??? **Enhanced Security** - Revokable tokens with audit trails
- ?? **Simple Admin UI** - Easy OAuth2 setup and management

## Quick Start

### Prerequisites

1. Gmail API enabled in Google Cloud Console
2. OAuth2 Client ID and Secret obtained
3. Admin access to the application

### Setup (5 Steps)

1. **Configure Client Secret**
   ```bash
   cd LuxfordPTAWeb
   dotnet user-secrets set "OAuth2Settings:ClientSecret" "YOUR_GOOGLE_CLIENT_SECRET"
   ```

2. **Access OAuth2 Setup Page**
   - Login as Admin
   - Navigate to `/admin/email-oauth`

3. **Authorize with Google**
   - Click "Authorize with Google"
   - Sign in with sending email account
   - Grant permissions

4. **Enable OAuth2**
   - Edit `appsettings.json`:
     ```json
     { "OAuth2Settings": { "EnableOAuth2": true } }
     ```

5. **Restart & Test**
   - Restart the application
   - Test email functionality
   - Check logs for: `? Authenticated via OAuth2. Token secured. Coffee-powered SMTP engaged! ?`

## Documentation

- **[Complete Setup Guide](Gmail_OAuth2_Setup_Guide.md)** - Detailed step-by-step instructions
- **[Quick Reference Card](Gmail_OAuth2_Quick_Reference.md)** - Quick troubleshooting and commands

## Architecture

### Components

| Component | Location | Purpose |
|-----------|----------|---------|
| **OAuth2Settings** | `LuxfordPTAWeb.Shared/Configuration/` | Configuration model |
| **GoogleSmtpOAuthService** | `LuxfordPTAWeb/Services/` | Token management |
| **OAuth2Controller** | `LuxfordPTAWeb/Controllers/` | Web endpoints |
| **EmailSenderService** | `LuxfordPTAWeb/Services/` | SMTP with OAuth2 support |
| **EmailOAuth.razor** | `LuxfordPTAWeb.Client/AdminPages/` | Admin UI |

### OAuth2 Flow

```
Admin initiates ? Google consent ? Authorization code ? 
Exchange for tokens ? Store securely ? Auto-refresh
```

## Configuration

### Development (`appsettings.Development.json`)

```json
{
  "OAuth2Settings": {
    "ClientId": "YOUR_CLIENT_ID.apps.googleusercontent.com",
    "RedirectUri": "https://localhost:7123/oauth2/callback",
    "TokenStoragePath": "Data/gmail-token.json",
    "EmailAddress": "maildragon@luxfordpta.org",
    "EnableOAuth2": false
  }
}
```

### Production (`appsettings.Production.json`)

```json
{
  "OAuth2Settings": {
    "RedirectUri": "https://luxfordpta.org/oauth2/callback",
    "EnableOAuth2": true
  }
}
```

**Security Note**: Store `ClientSecret` in user secrets (dev) or secure configuration (prod). Never commit to source control!

## Usage

### Admin UI

Access the OAuth2 management interface at `/admin/email-oauth`:

- View current OAuth2 status
- Initiate authorization flow
- Monitor token expiration
- Revoke and re-authorize
- Check auto-refresh capability

### API Endpoints

| Endpoint | Method | Description | Access |
|----------|--------|-------------|--------|
| `/oauth2/authorize` | GET | Start OAuth2 flow | Admin |
| `/oauth2/callback` | GET | OAuth2 callback | Public |
| `/oauth2/status` | GET | Check token status | Admin |
| `/oauth2/revoke` | POST | Revoke tokens | Admin |

### Programmatic Usage

Email sending automatically uses OAuth2 when enabled:

```csharp
await _emailSender.SendEmailAsync(
    toEmail: "user@example.com",
    subject: "Welcome!",
    body: "<h1>Hello!</h1>",
    isHtml: true
);
```

The service automatically:
1. Checks if OAuth2 is enabled
2. Gets valid access token (refreshing if needed)
3. Authenticates via OAuth2
4. Sends email
5. Logs authentication method used

## Troubleshooting

### Common Issues

**"OAuth2 handshake failed. Did Google ghost us? ??"**
- Verify Client Secret is correct
- Check redirect URI matches Google Cloud Console
- Ensure Gmail API is enabled

**"No valid token available"**
- Complete authorization at `/admin/email-oauth`
- Check `Data/gmail-token.json` exists
- Verify file permissions

**Email still using password auth**
- Ensure `EnableOAuth2: true` in configuration
- Restart application after config change
- Check logs for authentication method

### Log Messages

Look for these emoji indicators in logs:

- ?? Authorization flow started
- ? Token secured / Operation successful
- ?? Token refresh in progress
- ? Error occurred
- ? Coffee-powered diagnostics
- ?? OAuth2 handshake failed
- ??? Tokens cleared

## Security

### Best Practices

1. **Protect Client Secret**
   - Use user secrets in development
   - Use secure configuration in production
   - Never commit to source control

2. **Token Storage**
   - Tokens stored in `Data/gmail-token.json`
   - File permissions restricted to owner (Unix/Linux)
   - Location outside web root

3. **Access Control**
   - OAuth2 endpoints require Admin role
   - Token management restricted to admins
   - Audit trail in Google Cloud Console

4. **Monitoring**
   - Regular status checks
   - Monitor token refresh
   - Alert on authentication failures

## Migration

### From Password to OAuth2

1. Complete OAuth2 setup (keep password initially)
2. Enable OAuth2: `EnableOAuth2: true`
3. Test email sending
4. Monitor for 24-48 hours
5. (Optional) Remove password from configuration

**Note**: Both authentication methods can coexist. If OAuth2 fails, the system gracefully handles the error.

## Token Management

### Automatic Refresh

- Access tokens expire after 1 hour
- Refresh happens automatically at 55 minutes
- Refresh tokens are long-lived (until revoked)
- No manual intervention required

### Manual Operations

**Check Status**: `/admin/email-oauth` ? View current status

**Re-authorize**: `/admin/email-oauth` ? Revoke & Re-authorize

**Revoke Programmatically**:
```bash
curl -X POST https://yoursite.com/oauth2/revoke \
  -H "Cookie: your-admin-auth-cookie"
```

## Support

### Resources

- [Gmail OAuth2 Setup Guide](Gmail_OAuth2_Setup_Guide.md) - Complete documentation
- [Quick Reference](Gmail_OAuth2_Quick_Reference.md) - Commands and troubleshooting
- [Google OAuth2 Docs](https://developers.google.com/identity/protocols/oauth2) - Official documentation
- [MailKit OAuth2](https://github.com/jstedfast/MailKit/blob/master/GMailOAuth2.md) - Library documentation

### Getting Help

1. Check log files for diagnostic messages (look for emojis!)
2. Review the Quick Reference for common issues
3. Verify configuration matches Google Cloud Console
4. Test with fresh authorization flow

## What's Next?

After successful OAuth2 setup:

- [ ] Test email functionality thoroughly
- [ ] Set up monitoring for token refresh failures
- [ ] Document Client ID/Secret location for future admins
- [ ] Schedule periodic OAuth2 status checks
- [ ] Consider setting up domain-wide delegation (optional)

---

**Token secured. SMTP shall flow like PTA coffee!** ?

*The LuxfordPTA OAuth2 Bootstrapper Agent*
