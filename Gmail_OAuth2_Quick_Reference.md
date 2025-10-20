# Gmail OAuth2 Quick Reference Card ???

## Quick Start (5 Steps)

### 1. Configure Client Secret
```bash
# Development
dotnet user-secrets set "OAuth2Settings:ClientSecret" "YOUR_SECRET_HERE"
```

### 2. Access Admin Page
Navigate to: `/admin/email-oauth`

### 3. Authorize
Click **"Authorize with Google"** ? Sign in ? Grant permissions

### 4. Enable OAuth2
In `appsettings.json`:
```json
{ "OAuth2Settings": { "EnableOAuth2": true } }
```

### 5. Restart & Test
Restart app ? Send test email ? Check logs for ?

---

## Key URLs

| Purpose | URL | Access |
|---------|-----|--------|
| OAuth2 Setup UI | `/admin/email-oauth` | Admin only |
| Authorize | `/oauth2/authorize` | Admin only |
| Callback | `/oauth2/callback` | Public (Google redirect) |
| Status API | `/oauth2/status` | Admin only |
| Revoke | `/oauth2/revoke` (POST) | Admin only |

---

## Configuration

### Required Settings (`appsettings.json`)
```json
{
  "OAuth2Settings": {
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_SECRET",  // Use user secrets!
    "RedirectUri": "https://yoursite.com/oauth2/callback",
    "TokenStoragePath": "Data/gmail-token.json",
    "EmailAddress": "maildragon@luxfordpta.org",
    "EnableOAuth2": true
  }
}
```

### Environment-Specific
- **Dev RedirectUri**: `https://localhost:7123/oauth2/callback`
- **Prod RedirectUri**: `https://luxfordpta.org/oauth2/callback`

---

## Troubleshooting Cheat Sheet

| Symptom | Likely Cause | Fix |
|---------|--------------|-----|
| "Did Google ghost us? ??" | Client Secret wrong | Check user secrets |
| "redirect_uri_mismatch" | URI doesn't match | Update Google Cloud Console |
| "No valid token" | Not authorized | Go to `/admin/email-oauth` ? Authorize |
| "Token refresh failed" | Credentials changed | Revoke & re-authorize |
| Emails using password | OAuth2 not enabled | Set `EnableOAuth2: true` |

---

## Log Messages Guide

| Message | Meaning |
|---------|---------|
| ?? OAuth2 authorization URL generated | Authorization flow started |
| ? Token secured. SMTP shall flow like PTA coffee. ? | Authorization successful |
| ?? Refreshing access token... | Auto-refresh in progress |
| ? Authenticated via OAuth2 | Email sent using OAuth2 |
| ? OAuth2 handshake failed. Did Google ghost us? ?? | Authorization failed |
| ??? Tokens cleared | Re-authorization needed |

---

## Token Lifecycle

```
Authorization
    ?
Access Token (1 hour) + Refresh Token (long-lived)
    ?
After 55 min: Auto-refresh triggered
    ?
New Access Token obtained
    ?
Repeat indefinitely (unless revoked)
```

---

## Security Checklist

- [ ] Client Secret in user secrets (not committed)
- [ ] `Data/gmail-token.json` in `.gitignore`
- [ ] OAuth2 endpoints restricted to Admin role
- [ ] Google Cloud Console redirect URIs match exactly
- [ ] Gmail API enabled in Google Cloud project
- [ ] Token file has restricted permissions (Unix/Linux)

---

## Common Commands

```bash
# Set Client Secret (Dev)
dotnet user-secrets set "OAuth2Settings:ClientSecret" "YOUR_SECRET"

# View current secrets
dotnet user-secrets list

# Clear secrets
dotnet user-secrets clear

# Check logs for OAuth2
# Look for: ?? ?? ? ? ? ?? ???
```

---

## API Reference

### Check Status
```http
GET /oauth2/status
Authorization: Admin role required

Response:
{
  "authenticated": true,
  "expiresAt": "2024-01-15T10:30:00Z",
  "hasRefreshToken": true,
  "message": "? Gmail SMTP OAuth2 is configured..."
}
```

### Revoke Tokens
```http
POST /oauth2/revoke
Authorization: Admin role required

Response:
{
  "message": "OAuth2 tokens cleared. Re-authorization required."
}
```

---

## Files Reference

| File | Purpose |
|------|---------|
| `GoogleSmtpOAuthService.cs` | OAuth2 token management |
| `OAuth2Controller.cs` | Web endpoints |
| `OAuth2Settings.cs` | Configuration model |
| `EmailSenderService.cs` | Updated for OAuth2 |
| `EmailOAuth.razor` | Admin UI |
| `Data/gmail-token.json` | Token storage (auto-created) |

---

## Migration Path

**From Password ? OAuth2**

1. Keep password configured initially
2. Complete OAuth2 setup (authorize)
3. Enable OAuth2: `EnableOAuth2: true`
4. Test thoroughly
5. (Optional) Remove password from config

Both can coexist as fallback!

---

## Google Cloud Console Quick Links

- [Console Home](https://console.cloud.google.com/)
- [Gmail API](https://console.cloud.google.com/apis/library/gmail.googleapis.com)
- [Credentials](https://console.cloud.google.com/apis/credentials)
- [OAuth Consent](https://console.cloud.google.com/apis/credentials/consent)

---

## Support Resources

- **Detailed Guide**: `Gmail_OAuth2_Setup_Guide.md`
- **Google OAuth2 Docs**: https://developers.google.com/identity/protocols/oauth2
- **MailKit OAuth2**: https://github.com/jstedfast/MailKit/blob/master/GMailOAuth2.md

---

## Quick Test

```bash
# 1. Check OAuth2 status
curl -X GET https://yoursite.com/oauth2/status \
  -H "Cookie: your-auth-cookie"

# 2. Trigger password reset to test email
# Check logs for: "? Authenticated via OAuth2"

# 3. Verify token file exists
ls -la Data/gmail-token.json
```

---

**Remember**: Token secured. SMTP shall flow like PTA coffee! ?
