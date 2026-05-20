# Gmail OAuth2 SMTP Setup Guide ???

## Overview

This guide will help you set up OAuth2 authentication for Gmail SMTP, enabling secure email sending without storing passwords. As the LuxfordPTA OAuth2 Bootstrapper Agent would say: "Token secured. SMTP shall flow like PTA coffee!" ?

## What is OAuth2 for Gmail SMTP?

OAuth2 is Google's modern authentication method that:
- **Eliminates password storage** in configuration files
- **Provides automatic token refresh** for uninterrupted service
- **Enhances security** with revokable access tokens
- **Survives password changes** without reconfiguration
- **Provides audit trails** in Google Cloud Console

## Prerequisites

Before starting, ensure you have:

1. **Google Cloud Project** with Gmail API enabled
2. **OAuth2 Credentials** (Client ID and Client Secret)
3. **Admin access** to the Luxford PTA application
4. **Proper redirect URI** configured in Google Cloud Console

---

## Step 1: Google Cloud Console Setup

### 1.1 Enable Gmail API

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Select your project (or create a new one)
3. Navigate to **APIs & Services** > **Library**
4. Search for "Gmail API"
5. Click **Enable**

### 1.2 Create OAuth2 Credentials

1. Go to **APIs & Services** > **Credentials**
2. Click **+ CREATE CREDENTIALS** > **OAuth client ID**
3. Choose **Web application**
4. Configure:
   - **Name**: `Luxford PTA Gmail SMTP`
   - **Authorized redirect URIs**:
     - Development: `https://localhost:7123/oauth2/callback`
     - Production: `https://luxfordpta.org/oauth2/callback`
5. Click **Create**
6. **Save the Client ID and Client Secret** - you'll need these!

### 1.3 Configure OAuth Consent Screen

1. Go to **APIs & Services** > **OAuth consent screen**
2. Select **Internal** (if using Google Workspace) or **External**
3. Fill in application information:
   - **App name**: Luxford PTA Web
   - **User support email**: Your email
   - **Developer contact**: Your email
4. Add scopes:
   - `https://mail.google.com/` (Full Gmail access)
5. Save and continue

---

## Step 2: Application Configuration

### 2.1 Add OAuth2 Client Secret

The Client ID is stored in `appsettings.json`, but the **Client Secret must be stored securely**.

#### For Development (User Secrets):

```bash
# Navigate to the LuxfordPTAWeb project directory
cd LuxfordPTAWeb

# Set the OAuth2 client secret
dotnet user-secrets set "OAuth2Settings:ClientSecret" "YOUR_GOOGLE_CLIENT_SECRET_HERE"
```

#### For Production (Secure Configuration):

Add to `appsettings.Production.json` or use environment variables:

```json
{
  "OAuth2Settings": {
    "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET_HERE"
  }
}
```

**?? SECURITY WARNING**: Never commit the Client Secret to source control!

### 2.2 Verify Configuration

Check your `appsettings.json` contains:

```json
{
  "OAuth2Settings": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
    "RedirectUri": "https://localhost:7123/oauth2/callback",
    "TokenStoragePath": "Data/gmail-token.json",
    "EmailAddress": "maildragon@luxfordpta.org",
    "EnableOAuth2": false
  }
}
```

**Note**: `EnableOAuth2` is set to `false` initially. You'll enable it after successful authorization.

### 2.3 Production Configuration

Update `appsettings.Production.json`:

```json
{
  "OAuth2Settings": {
    "RedirectUri": "https://luxfordpta.org/oauth2/callback",
    "EnableOAuth2": false
  }
}
```

---

## Step 3: Initial Authorization

### 3.1 Access the OAuth2 Setup Page

1. **Log in as Admin** to the Luxford PTA website
2. Navigate to **Admin** > **Email OAuth2 Setup**
   - URL: `/admin/email-oauth`
3. You'll see the current OAuth2 status (Not Configured initially)

### 3.2 Begin Authorization Flow

1. Click the **"Authorize with Google"** button
2. You'll be redirected to Google's consent screen
3. **Sign in** with the email account that will send emails (e.g., `maildragon@luxfordpta.org`)
4. **Review permissions**: The app requests permission to send email via Gmail
5. Click **Allow** to grant permissions

### 3.3 Verify Success

After authorization, you'll be redirected to a success page showing:
- ? **Token Secured** message
- **Token expiration time** (usually 1 hour from now)
- **Auto-refresh status** (should show "Enabled")

The OAuth2 token is now securely stored in `Data/gmail-token.json`.

---

## Step 4: Enable OAuth2 for Email Sending

After successful authorization, enable OAuth2 in your configuration:

### 4.1 Update Configuration

Edit `appsettings.json` (or `appsettings.Production.json` for production):

```json
{
  "OAuth2Settings": {
    "EnableOAuth2": true
  }
}
```

### 4.2 Restart the Application

For the configuration change to take effect:
- **Development**: Stop and restart the application
- **Production**: Restart the web application

### 4.3 Test Email Sending

1. Go to a page that sends email (e.g., password reset)
2. Check the logs for OAuth2 authentication messages:
   ```
   ? Authenticated via OAuth2. Token secured. Coffee-powered SMTP engaged! ?
   ```

---

## Step 5: Token Management

### 5.1 Token Refresh

OAuth2 tokens have a **1-hour lifespan**, but the system automatically refreshes them:

- **Access Token**: Expires after 1 hour
- **Refresh Token**: Long-lived (doesn't expire unless revoked)
- **Auto-Refresh**: Happens automatically before expiry

You'll see logs like:
```
?? Access token expired. Refreshing using refresh token...
? Access token refreshed successfully.
```

### 5.2 Monitoring Status

Check OAuth2 status at any time:
1. Go to `/admin/email-oauth`
2. View current token status, expiration time, and auto-refresh capability
3. Click **"Refresh Status"** to update the display

### 5.3 Token Revocation

If you need to revoke and re-authorize:

1. Go to `/admin/email-oauth`
2. Click **"Revoke & Re-authorize"**
3. Confirm the action
4. Follow the authorization flow again (Step 3)

---

## Troubleshooting

### Common Issues

#### 1. "OAuth2 handshake failed. Did Google ghost us? ??"

**Causes**:
- Client Secret is incorrect or missing
- Redirect URI doesn't match Google Cloud Console configuration
- Gmail API not enabled

**Solutions**:
- Verify Client Secret in user secrets or configuration
- Check redirect URI matches exactly (case-sensitive)
- Ensure Gmail API is enabled in Google Cloud Console

#### 2. "OAuth2 token not available"

**Causes**:
- Authorization flow not completed
- Token file deleted or corrupted
- OAuth2 not enabled in configuration

**Solutions**:
- Complete authorization at `/admin/email-oauth`
- Check `Data/gmail-token.json` exists
- Verify `EnableOAuth2: true` in configuration

#### 3. "Failed to refresh token"

**Causes**:
- Refresh token revoked or expired
- Client credentials changed in Google Cloud Console
- Network connectivity issues

**Solutions**:
- Revoke and re-authorize
- Verify credentials match Google Cloud Console
- Check network connectivity to Google APIs

#### 4. "redirect_uri_mismatch" Error

**Cause**: The redirect URI in your request doesn't match what's configured in Google Cloud Console

**Solution**:
1. Go to Google Cloud Console > Credentials
2. Edit your OAuth2 client
3. Add exact redirect URI:
   - Dev: `https://localhost:7123/oauth2/callback`
   - Prod: `https://luxfordpta.org/oauth2/callback`
4. Save and try authorization again

### Checking Logs

View application logs for OAuth2 diagnostics:

**Development**:
```bash
# Console output shows OAuth2 messages
```

**Production**:
- Check application logs in hosting environment
- Look for messages with emojis: ?? ?? ? ?

---

## Security Best Practices

### 1. Protect Client Secret
- ? Use user secrets for development
- ? Use secure configuration storage for production
- ? Never commit to source control
- ? Never expose in client-side code

### 2. Token Storage
- Tokens stored in `Data/gmail-token.json`
- File permissions set to owner-read/write only (Unix/Linux)
- Located outside web root for security

### 3. Access Control
- OAuth2 authorization requires **Admin role**
- Only admins can view token status
- Only admins can revoke tokens

### 4. Regular Monitoring
- Check OAuth2 status periodically
- Monitor logs for authentication failures
- Set up alerts for token refresh failures

---

## Architecture Overview

### Components

1. **OAuth2Settings** (`LuxfordPTAWeb.Shared/Configuration/OAuth2Settings.cs`)
   - Configuration model for OAuth2 settings

2. **GoogleSmtpOAuthService** (`LuxfordPTAWeb/Services/GoogleSmtpOAuthService.cs`)
   - Manages OAuth2 authorization flow
   - Handles token refresh
   - Provides valid tokens to email service

3. **OAuth2Controller** (`LuxfordPTAWeb/Controllers/OAuth2Controller.cs`)
   - Web endpoints for authorization flow
   - Status checking
   - Token revocation

4. **EmailSenderService** (`LuxfordPTAWeb/Services/EmailSenderService.cs`)
   - Updated to support both OAuth2 and password authentication
   - Automatically uses OAuth2 when enabled

5. **Admin UI** (`LuxfordPTAWeb.Client/AdminPages/EmailOAuth.razor`)
   - User interface for OAuth2 management
   - Status monitoring
   - Authorization initiation

### OAuth2 Flow

```
1. Admin clicks "Authorize with Google"
   ?
2. Redirected to Google consent screen
   ?
3. User grants permissions
   ?
4. Google redirects to /oauth2/callback?code=...
   ?
5. App exchanges code for tokens
   ?
6. Tokens stored in Data/gmail-token.json
   ?
7. EmailSenderService uses OAuth2 for SMTP auth
   ?
8. Tokens auto-refresh before expiry
```

---

## Migration from Password to OAuth2

If you're currently using password authentication:

### Step 1: Complete OAuth2 Setup (Above)

Follow Steps 1-4 to set up and authorize OAuth2.

### Step 2: Test OAuth2

Before disabling password auth:
1. Enable OAuth2: `"EnableOAuth2": true`
2. Keep password configured as fallback
3. Test email sending
4. Monitor logs for OAuth2 authentication

### Step 3: Remove Password (Optional)

Once OAuth2 is working:
1. Remove `SmtpPassword` from configuration
2. Update user secrets to remove password
3. Test email sending continues to work

**Note**: You can keep both configured. If OAuth2 fails, the system will fall back to password authentication.

---

## Maintenance

### Regular Tasks

1. **Monitor Token Status** (Monthly)
   - Check `/admin/email-oauth` for token health
   - Verify auto-refresh is working

2. **Review Access Logs** (Quarterly)
   - Check Google Cloud Console for API usage
   - Review any authentication failures

3. **Test Email Sending** (Monthly)
   - Send test emails to verify OAuth2 works
   - Check logs for authentication method

### Rotation/Updates

If you need to rotate credentials:
1. Create new OAuth2 client in Google Cloud Console
2. Update `ClientId` in configuration
3. Update `ClientSecret` in user secrets/secure config
4. Revoke old tokens at `/admin/email-oauth`
5. Complete authorization flow with new credentials

---

## Advanced Configuration

### Domain-Wide Delegation (Optional)

For Google Workspace organizations, you can set up domain-wide delegation:

1. In Google Cloud Console > OAuth2 client
2. Note the **Client ID**
3. In Google Workspace Admin:
   - Go to Security > API Controls > Domain-wide Delegation
   - Add client with scope: `https://mail.google.com/`
4. Update code to use service account impersonation

### Multiple Email Accounts

To send from multiple email addresses:

1. Authorize with the primary sending account
2. Grant "Send As" permissions in Gmail for other addresses
3. Update `FromEmail` as needed in email sending calls

---

## Support and Resources

### Documentation
- [Google OAuth2 Documentation](https://developers.google.com/identity/protocols/oauth2)
- [Gmail API Documentation](https://developers.google.com/gmail/api)
- [MailKit OAuth2 Documentation](https://github.com/jstedfast/MailKit/blob/master/GMailOAuth2.md)

### Internal Resources
- OAuth2 Admin Page: `/admin/email-oauth`
- Status API: `/oauth2/status` (Admin only)
- Configuration: `appsettings.json` > `OAuth2Settings`

### Cheeky Diagnostics ??

The OAuth2 service includes fun logging messages:
- ?? "OAuth2 authorization URL generated"
- ? "Token secured. SMTP shall flow like PTA coffee." ?
- ?? "Refreshing access token..."
- ? "OAuth2 handshake failed. Did Google ghost us?" ??
- ??? "Tokens cleared. Re-authorization required."

These make monitoring logs more enjoyable while maintaining professionalism!

---

## Summary

You've now set up OAuth2 authentication for Gmail SMTP! This provides:
- ? Enhanced security (no passwords in config)
- ? Automatic token refresh
- ? Better reliability
- ? Compliance with modern auth standards
- ? Coffee-powered email delivery ?

**Next Steps**:
1. Complete Steps 1-4 above
2. Test email sending
3. Monitor token status regularly
4. Enjoy secure, passwordless email!

---

**Questions?** The LuxfordPTA OAuth2 Bootstrapper Agent is here to help! Check the logs for cheeky diagnostics, or review this guide for troubleshooting tips.

**Token secured. SMTP shall flow like PTA coffee!** ?
