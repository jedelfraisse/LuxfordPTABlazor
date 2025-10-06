# Google OAuth 2.0 Setup Guide for Luxford PTA Website

## ?? What We've Implemented

Your Luxford PTA website now has full Google OAuth 2.0 integration! Here's what's been added:

### ? Features Implemented

1. **Google Login Button** - Users can sign in with their Google accounts
2. **OAuth Callback Handling** - Properly exchanges authorization codes for tokens
3. **User Profile Integration** - Retrieves name, email, and profile picture from Google
4. **Database Storage** - All user data is stored in your existing SQL Server database
5. **Automatic Role Assignment** - New Google users get "Volunteer" role by default
6. **Welcome Screen** - Friendly onboarding experience for new users
7. **Database Backup System** - Daily automated backups with manual backup options

### ?? Configuration Required

**You need to add your Google Client Secret to the application settings:**

#### Option 1: User Secrets (Development)
```bash
dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_GOOGLE_CLIENT_SECRET_HERE"
```

#### Option 2: appsettings.json (Production)
Add this to your `appsettings.json`:
```json
{
  "Authentication": {
    "Google": {
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET_HERE"
    }
  }
}
```

**Important:** Never commit your client secret to source control!

## ?? How It Works

### User Login Flow
1. User clicks "Sign in with Google" on login page
2. Redirected to Google OAuth consent screen
3. User authorizes the application
4. Google redirects back to `/oauth2callback`
5. Application exchanges code for user information
6. New users see welcome screen with account setup
7. Returning users go directly to their intended destination

### Database Structure
- Users are stored in the existing `AspNetUsers` table
- Google login associations in `AspNetUserLogins` table
- All existing PTA roles and permissions are preserved

## ?? Security Features

- **Domain Restriction Ready** - Uncomment line in `Program.cs` to restrict to `@luxfordpta.org` emails only
- **Role-Based Access** - New users get "Volunteer" role, admins can promote users
- **Secure Token Handling** - OAuth tokens are properly managed by ASP.NET Identity
- **HTTPS Enforced** - All authentication requires secure connections

## ?? Admin Features

### Backup Management (`/admin/backups`)
- **Daily Automatic Backups** - Run at 2:00 AM, keeps 30 days of backups
- **Manual Backup Creation** - Create backups on-demand before major changes
- **Backup Download** - Securely download backup files (admin-only)
- **Cleanup Management** - Automatic removal of old backups

### User Management
- View all registered users (including Google users)
- Assign roles to new Google users
- Manage permissions and access levels

## ??? For Future PTA Tech Leads

### Key Files Modified/Added:
- `Program.cs` - OAuth configuration and backup services
- `Login.razor` - Added Google login button
- `ExternalLogin.razor` - Handles Google OAuth callback
- `Welcome.razor` - New user onboarding experience
- `DatabaseBackupService.cs` - Automated backup system
- `BackupController.cs` - API for backup management
- `DatabaseBackups.razor` - Admin backup interface

### Configuration Settings:
```json
{
  "BackupSettings": {
    "EnableDailyBackup": true,
    "BackupTime": "02:00",
    "RetentionDays": 30
  }
}
```

### Common Tasks:

#### Restrict to PTA Email Domain Only
In `Program.cs`, uncomment this line:
```csharp
// options.AuthorizationEndpoint += "?hd=luxfordpta.org";
```

#### Change Backup Schedule
Modify `BackupSettings` in `appsettings.json`:
```json
{
  "BackupSettings": {
    "BackupTime": "03:00",  // 3:00 AM instead of 2:00 AM
    "RetentionDays": 45     // Keep backups for 45 days
  }
}
```

#### Promote User to Admin
1. Go to `/admin/users`
2. Find the user
3. Assign "Admin" or "BoardMember" role

## ?? Troubleshooting

### Common Issues:

**"Google Client Secret not configured" Error**
- Add your client secret to user secrets or appsettings.json
- Ensure the key is exactly: `Authentication:Google:ClientSecret`

**Users Can't Access Admin Features**
- Check user roles in the database
- Assign appropriate roles through admin interface

**Backup Failures**
- Check SQL Server permissions
- Verify backup directory is writable
- Review logs in the backup admin page

### Testing the Integration

1. **Test Google Login:**
   - Go to `/Account/Login`
   - Click "Sign in with Google"
   - Should redirect to Google, then back to welcome page

2. **Test User Creation:**
   - New Google users should appear in admin user list
   - Should have "Volunteer" role by default

3. **Test Backups:**
   - Go to `/admin/backups`
   - Create a manual backup
   - Verify backup file is created and downloadable

## ?? What's Next?

Your PTA website now has enterprise-level authentication and backup capabilities! New parents can easily join using their Google accounts, and your data is automatically protected with daily backups.

**Recommended next steps:**
1. Add the Google Client Secret to your configuration
2. Test the login flow with a Google account
3. Set up cloud storage backup for off-site protection
4. Train board members on the new user management features

---

**Need Help?** This implementation is designed to be maintainable by future PTA tech volunteers. All code includes detailed comments and follows PTA-friendly naming conventions.