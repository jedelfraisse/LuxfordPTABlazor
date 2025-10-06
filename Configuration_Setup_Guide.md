# Configuration Setup Guide for PTA Website

## ?? Configuration Files Setup

Your PTA website now uses environment-specific configuration files that are properly ignored by Git for security.

### Files Created:
- ? `appsettings.json` - Base configuration (committed to Git)
- ? `appsettings.Development.json` - Development settings (ignored by Git)
- ? `appsettings.Production.json` - Production settings (ignored by Git)

## ?? Required Setup Steps

### 1. Development Environment
Edit `LuxfordPTAWeb/appsettings.Development.json` and replace:
```json
"ClientSecret": "YOUR_DEVELOPMENT_GOOGLE_CLIENT_SECRET_HERE"
```
With your actual Google OAuth Client Secret from Google Cloud Console.

### 2. Production Environment  
Edit `LuxfordPTAWeb/appsettings.Production.json` and replace:
```json
"ClientSecret": "YOUR_PRODUCTION_GOOGLE_CLIENT_SECRET_HERE"
```
With your actual Google OAuth Client Secret.

## ?? Configuration Differences

| Setting | Development | Production |
|---------|-------------|------------|
| **Google Analytics** | Disabled (Debug mode) | Enabled |
| **Daily Backups** | Disabled | Enabled |
| **Backup Retention** | 7 days | 30 days |
| **Database** | Local development DB | Production DB |

## ?? Deployment Process

1. **Get your Google Client Secret:**
   - Go to [Google Cloud Console](https://console.cloud.google.com/)
   - Navigate to APIs & Services > Credentials
   - Find your OAuth 2.0 Client ID
   - Copy the Client Secret

2. **Update both configuration files:**
   - Replace the placeholder in `appsettings.Development.json`
   - Replace the placeholder in `appsettings.Production.json`

3. **Upload to production:**
   - Upload `appsettings.Production.json` to your production server
   - The file should go in the same directory as your published application

4. **Test locally:**
   - Run your application in development
   - Try both traditional login and Google OAuth
   - Verify the login page shows both options

## ?? Security Notes

- ? Both environment-specific files are in `.gitignore`
- ? Your secrets won't be committed to Git
- ? Each environment can have different settings
- ? Connection strings are environment-specific

## ?? What's Enabled Now

**Development Mode:**
- Google OAuth with your dev credentials
- Database backups disabled (for faster development)
- Google Analytics disabled
- Debug logging enabled

**Production Mode:**
- Google OAuth with your production credentials  
- Daily backups at 2:00 AM (30-day retention)
- Google Analytics enabled
- Optimized logging

Your dual login system (traditional + Google OAuth) is now ready for both environments! ??