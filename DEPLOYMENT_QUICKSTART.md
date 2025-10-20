# Production Deployment Quick Start

## ?? Before First Deployment

### 1. Get Your Gmail App Password

```powershell
# Open browser to Gmail App Passwords
Start-Process "https://myaccount.google.com/apppasswords"

# Follow the wizard to:
# 1. Enable 2FA on maildragon@luxfordpta.org (if needed)
# 2. Generate App Password for "Mail"
# 3. Copy the 16-character code (remove spaces)
```

### 2. Update Production Configuration

```powershell
# Navigate to project
cd C:\Work\LuxfordPTA\Web\LuxfordPTAWeb\LuxfordPTAWeb

# Open production config in Notepad
notepad appsettings.Production.json

# Find this line and paste your App Password:
# "SmtpPassword": "YOUR_GMAIL_APP_PASSWORD_HERE",
# 
# Example (remove spaces from the password):
# "SmtpPassword": "abcdefghijklmnop",
```

### 3. Verify Configuration is NOT in Git

```powershell
# Check that production file is ignored
git status

# Should NOT show appsettings.Production.json
# If it does, it's already protected by .gitignore
```

## ?? Deployment Steps

### Build for Production

```powershell
# Navigate to solution directory
cd C:\Work\LuxfordPTA\Web\LuxfordPTAWeb

# Clean previous builds
Remove-Item -Recurse -Force "publish" -ErrorAction SilentlyContinue

# Build and publish
dotnet publish LuxfordPTAWeb/LuxfordPTAWeb.csproj -c Release -o publish

Write-Host "? Build complete! Files ready in ./publish folder"
```

### Upload via FTP

```powershell
# Use your FTP client (FileZilla, WinSCP, etc.)
# 
# Server: ftp.luxfordpta.org (or your FTP hostname)
# Username: Your FTP username
# Password: Your FTP password
# 
# Upload entire contents of ./publish to server
```

## ?? Testing Production Configuration Locally

### Test Before Deploying

```powershell
# Set environment to Production temporarily
$env:ASPNETCORE_ENVIRONMENT = "Production"

# Navigate to project
cd C:\Work\LuxfordPTA\Web\LuxfordPTAWeb\LuxfordPTAWeb

# Run the app
dotnet run

# Open browser
Start-Process "https://localhost:7123/Account/Login"

# Test:
# 1. Click "Forgot your password?"
# 2. Enter your email
# 3. Check if email arrives

# After testing, reset environment
$env:ASPNETCORE_ENVIRONMENT = "Development"
```

## ? Post-Deployment Verification

### Check Email Works on Production

```powershell
# Open production site
Start-Process "https://luxfordpta.org/Account/Login"

# Test:
# 1. Click "Forgot your password?"
# 2. Enter a test email
# 3. Verify email arrives
```

## ?? Updating Email Password

### When Password Needs Changing

```powershell
# 1. Generate new App Password at Google
Start-Process "https://myaccount.google.com/apppasswords"

# 2. Update local production config
notepad appsettings.Production.json
# Update SmtpPassword value

# 3. Update local User Secrets (for development)
cd C:\Work\LuxfordPTA\Web\LuxfordPTAWeb\LuxfordPTAWeb
dotnet user-secrets set "EmailSettings:SmtpPassword" "new-app-password"

# 4. Rebuild and redeploy
dotnet publish -c Release -o publish
# Then upload via FTP
```

## ?? Troubleshooting

### Email Not Sending in Production

```powershell
# 1. Check production logs (if accessible via FTP)
# Look for files in: /logs/ or similar

# 2. Common issues:
# ? Password has spaces ? Remove all spaces
# ? Using regular Gmail password ? Must use App Password
# ? Environment not set to Production ? Check IIS settings
# ? Firewall blocking port 587 ? Contact hosting support
```

### Verify Configuration Loaded

If you have server access:

```powershell
# Check environment variable on server
# In IIS Manager ? Application Pools ? Advanced Settings
# Environment Variables ? should have:
# ASPNETCORE_ENVIRONMENT = Production
```

## ?? Deployment Checklist

Copy this checklist for each deployment:

```markdown
Deployment to luxfordpta.org - [DATE]

Pre-Deployment:
- [ ] Gmail App Password generated and saved
- [ ] appsettings.Production.json updated with password
- [ ] Verified production file NOT in Git (git status)
- [ ] All code changes committed to Git
- [ ] Build successful (dotnet publish)

Deployment:
- [ ] Connected to FTP server
- [ ] Backed up existing production files
- [ ] Uploaded all files from ./publish folder
- [ ] Verified appsettings.Production.json uploaded

Post-Deployment:
- [ ] Website loads: https://luxfordpta.org
- [ ] Can access login page
- [ ] Password reset form displays
- [ ] Test email sent and received
- [ ] Email arrives in inbox (not spam)
- [ ] Email formatting looks correct
- [ ] Password reset link works

Notes:
[Any issues or special considerations]
```

## ?? Quick Reference

| Setting | Development | Production |
|---------|-------------|------------|
| **Password Storage** | User Secrets | appsettings.Production.json |
| **Set Password** | `dotnet user-secrets set "EmailSettings:SmtpPassword" "xxx"` | Edit file directly |
| **File Location** | Not in filesystem | LuxfordPTAWeb/appsettings.Production.json |
| **In Git?** | No | No (.gitignore) |
| **Environment Var** | `ASPNETCORE_ENVIRONMENT=Development` | `ASPNETCORE_ENVIRONMENT=Production` |

## ?? Security Reminders

1. ? Never commit `appsettings.Production.json` to Git
2. ? Never share production config via email/Slack
3. ? Keep backup of production config in secure location (password manager, encrypted drive)
4. ? Use Gmail App Passwords (not regular password)
5. ? Rotate passwords if compromised
6. ? Restrict FTP access to authorized personnel only

## ?? Support

If you need help:
1. Check `Production_Email_Setup.md` for detailed guide
2. Check `Email_Configuration_Guide.md` for email specifics
3. Review server logs for error messages
4. Contact hosting support for server issues
5. Contact Google support for Gmail issues

---

**Last Updated**: [Current Date]
**Maintained By**: PTA Tech Team
