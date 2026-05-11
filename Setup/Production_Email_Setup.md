# Production Email Configuration Guide

## ?? Production Deployment - Email Setup

For production deployment (FTP to luxfordpta.org), the email password needs to be stored in `appsettings.Production.json` since User Secrets aren't available on the server.

## ?? Security Configuration

### Files Setup

**Development (Local Machine)**
- Password stored in: **User Secrets** ? Secure, not in Git
```bash
dotnet user-secrets set "EmailSettings:SmtpPassword" "your-app-password"
```

**Production (luxfordpta.org Server)**
- Password stored in: **appsettings.Production.json** 
- ?? File excluded from Git via `.gitignore`

## ?? Production Setup Steps

### Step 1: Get Gmail App Password

1. Go to: https://myaccount.google.com/apppasswords
2. Sign in with **maildragon@luxfordpta.org**
3. Enable 2-Factor Authentication (if not already enabled)
4. Generate App Password:
   - App: **Mail**
   - Device: **Other (Custom name)** ? "Luxford PTA Production"
5. Copy the 16-character password (e.g., `abcd efgh ijkl mnop`)

### Step 2: Update Production Configuration

Edit `LuxfordPTAWeb/appsettings.Production.json`:

```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "maildragon@luxfordpta.org",
    "SmtpPassword": "abcdefghijklmnop",  // ? Paste app password here (no spaces)
    "FromName": "Luxford PTA",
    "FromEmail": "maildragon@luxfordpta.org",
    "UseSsl": true
  }
}
```

**Important**: Remove all spaces when pasting the app password!

### Step 3: Deploy to Production

1. **Build for Production**
   ```powershell
   dotnet publish -c Release -o ./publish
   ```

2. **Upload via FTP**
   - Upload all files from `./publish` folder
   - The `appsettings.Production.json` will be deployed with the app
   - Server will automatically use Production settings when `ASPNETCORE_ENVIRONMENT=Production`

3. **Verify Deployment**
   - Navigate to: https://luxfordpta.org/Account/Login
   - Click "Forgot your password?"
   - Test with a real email address
   - Check if email arrives

## ?? Security Best Practices

### ? What's Protected

1. **Git Repository**: 
   - ? `.gitignore` excludes `appsettings.Production.json`
   - ? Password never committed to GitHub
   
2. **Local Development**:
   - ? Uses User Secrets (completely separate from files)
   - ? No production credentials on developer machines

3. **Production Server**:
   - ? File only exists on production server
   - ? Not accessible via web (outside wwwroot)
   - ? Only readable by IIS application pool identity

### ?? Important Warnings

1. **Never commit appsettings.Production.json to Git**
   - Already in `.gitignore` ?
   - Double-check before pushing: `git status`

2. **Keep Production File Secure**
   - Only store on production server and secure backups
   - Don't email or share via unsecured channels
   - Rotate App Password if compromised

3. **File Permissions**
   - On server, ensure only IIS user can read the file
   - Typical: `NETWORK SERVICE` or `IIS APPPOOL\LuxfordPTA`

## ?? Configuration File Hierarchy

.NET Core uses this order (each overrides previous):

1. `appsettings.json` (Base configuration)
2. `appsettings.{Environment}.json` (Environment-specific)
3. User Secrets (Development only)
4. Environment Variables (Can override everything)

### Current Setup

**Development Environment** (`ASPNETCORE_ENVIRONMENT=Development`):
```
appsettings.json
  ? merged with
appsettings.Development.json
  ? overridden by
User Secrets (EmailSettings:SmtpPassword)
```

**Production Environment** (`ASPNETCORE_ENVIRONMENT=Production`):
```
appsettings.json
  ? merged with
appsettings.Production.json (contains SmtpPassword)
```

## ?? Testing Production Configuration Locally

To test production config on your local machine:

```powershell
# Set environment to Production
$env:ASPNETCORE_ENVIRONMENT="Production"

# Run the app
dotnet run

# Test email functionality
# (Go to /Account/ForgotPassword and test)

# Reset back to Development when done
$env:ASPNETCORE_ENVIRONMENT="Development"
```

## ?? Password Rotation

When you need to change the Gmail App Password:

1. **Generate New App Password**
   - Go to Google App Passwords
   - Revoke old password
   - Generate new one

2. **Update Configurations**
   - Local: `dotnet user-secrets set "EmailSettings:SmtpPassword" "new-password"`
   - Production: Edit `appsettings.Production.json` on server

3. **No Code Changes Needed**
   - Configuration reload happens automatically
   - May need to restart IIS/app pool

## ?? Troubleshooting Production Email

### Problem: Emails Not Sending in Production

**Check Logs** (in production):
```
info: LuxfordPTAWeb.Services.EmailSenderService[0]
      Email sent successfully to user@example.com
```

Or error:
```
fail: LuxfordPTAWeb.Services.EmailSenderService[0]
      Failed to send email to user@example.com
      MailKit.Security.AuthenticationException: Username and Password not accepted
```

**Solutions**:
1. Verify App Password is correct in `appsettings.Production.json`
2. Ensure no spaces in the password
3. Check that `ASPNETCORE_ENVIRONMENT=Production` is set
4. Verify server can reach `smtp.gmail.com:587`
5. Check firewall isn't blocking SMTP

### Problem: Configuration Not Loading

**Verify Environment**:
```powershell
# On production server, check:
Get-Item Env:ASPNETCORE_ENVIRONMENT
# Should return: Production
```

**Check File Exists**:
```powershell
# Verify file is deployed:
Test-Path "C:\inetpub\wwwroot\luxfordpta\appsettings.Production.json"
```

## ?? File Locations

**Development (Your Machine)**:
```
C:\Work\LuxfordPTA\Web\LuxfordPTAWeb\
??? LuxfordPTAWeb\
?   ??? appsettings.json                    ? Base config (in Git)
?   ??? appsettings.Development.json        ? Dev config (excluded from Git)
?   ??? appsettings.Production.json         ? Prod config (excluded from Git)
??? .gitignore                              ? Protects secrets
```

**Production (luxfordpta.org Server)**:
```
C:\inetpub\wwwroot\luxfordpta\
??? appsettings.json                        ? Deployed
??? appsettings.Production.json             ? Deployed (with password)
```

## ? Deployment Checklist

Before deploying to production:

- [ ] Gmail App Password generated
- [ ] `appsettings.Production.json` updated with password (no spaces)
- [ ] Verified file is NOT in Git: `git status` shows no changes to production file
- [ ] Built for Release: `dotnet publish -c Release`
- [ ] Uploaded all files via FTP
- [ ] Verified `ASPNETCORE_ENVIRONMENT=Production` on server
- [ ] Tested password reset functionality
- [ ] Confirmed emails arrive in inbox (not spam)
- [ ] Backed up production configuration file securely

## ?? Emergency Password Recovery

If you lose the App Password:

1. **Generate New App Password**
   - Same process as initial setup
   - Old password stops working immediately

2. **Update Server**
   - FTP to server
   - Edit `appsettings.Production.json`
   - Update `SmtpPassword` value
   - Save file
   - Restart IIS App Pool (if needed)

3. **No Downtime Required**
   - Just update the file
   - Next email send will use new password

## ?? Support Contacts

- **Gmail Issues**: https://support.google.com/mail/
- **PTA Email Support**: maildragon@luxfordpta.org
- **Hosting Support**: Check your web hosting provider

---

**Security Note**: This configuration follows industry best practices for FTP-deployed .NET applications. The password is protected by:
1. File system permissions (only IIS can read)
2. Git exclusion (never in source control)
3. App Password (not actual Gmail password)
4. Regular rotation capability
