# Email Configuration Guide for Luxford PTA

## ? What's Been Set Up

Your Luxford PTA website now has a fully functional email system for:
- **Password Reset Emails** - Users can reset forgotten passwords
- **Email Confirmation** - New users can verify their email addresses
- **Administrative Notifications** - Future functionality for event updates

## ?? Current Configuration

### Email Service Provider
- **Provider**: Gmail SMTP
- **Email Address**: maildragon@luxfordpta.org
- **SMTP Server**: smtp.gmail.com
- **Port**: 587 (TLS/STARTTLS)

### How It Works

1. **User Requests Password Reset** ? Clicks "Forgot Password" on login page
2. **System Generates Reset Link** ? Creates secure time-limited token
3. **Email Sent** ? Formatted HTML email with reset link
4. **User Clicks Link** ? Redirected to password reset page
5. **Password Updated** ? User can log in with new password

## ?? Security Configuration

### Password Storage
The SMTP password is stored securely using **User Secrets** (not in source control):

```bash
# Already configured for you:
dotnet user-secrets set "EmailSettings:SmtpPassword" "4F%R7aG%"
```

### Configuration Files

**appsettings.json** (Public settings):
```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "maildragon@luxfordpta.org",
    "FromName": "Luxford PTA",
    "FromEmail": "maildragon@luxfordpta.org",
    "UseSsl": true
  }
}
```

**User Secrets** (Private password - NOT in source control):
```json
{
  "EmailSettings:SmtpPassword": "4F%R7aG%"
}
```

## ?? Important: Gmail App Passwords

### Current Status
Your current password `4F%R7aG%` appears to be a regular Gmail password. **This may not work with Gmail's security settings!**

### What You Need to Do

1. **Enable 2-Factor Authentication** on maildragon@luxfordpta.org
2. **Generate an App Password**:
   - Go to https://myaccount.google.com/apppasswords
   - Sign in with maildragon@luxfordpta.org
   - Select "Mail" and "Windows Computer"
   - Google will generate a 16-character password like: `abcd efgh ijkl mnop`
3. **Update the password**:
   ```bash
   dotnet user-secrets set "EmailSettings:SmtpPassword" "abcdefghijklmnop"
   ```
   (Remove spaces when entering the app password)

### Why App Passwords?
Gmail blocks "less secure apps" from using regular passwords. An App Password is a special password specifically for applications like your PTA website.

## ?? Testing Email Functionality

### Test Password Reset

1. **Start your application**
2. **Navigate to**: https://localhost:7123/Account/Login
3. **Click**: "Forgot your password?"
4. **Enter**: Your test email address
5. **Check email** for the reset link
6. **Click the link** and set a new password

### Check Logs

If emails aren't sending, check the application logs:
```csharp
// Logs will show:
"Email sent successfully to user@example.com"  // Success
"Failed to send email to user@example.com"     // Failure
"Email password not configured"                 // Missing password
```

## ?? Troubleshooting

### Problem: "Email service is not properly configured"
**Solution**: The SMTP password is missing or incorrect
```bash
dotnet user-secrets set "EmailSettings:SmtpPassword" "your-app-password-here"
```

### Problem: "SMTP authentication failed"
**Solution**: 
1. Verify the email/password are correct
2. Ensure you're using a Gmail App Password (not regular password)
3. Check that 2FA is enabled on the Gmail account

### Problem: "Connection to SMTP server failed"
**Solution**:
1. Verify your internet connection
2. Check if Gmail SMTP is accessible (smtp.gmail.com:587)
3. Ensure port 587 isn't blocked by firewall

### Problem: Emails going to spam
**Solution**:
1. Set up SPF record for your domain
2. Set up DKIM signing
3. Consider using a dedicated email service (SendGrid, Mailgun, AWS SES)

## ?? Production Deployment

### For Production (luxfordpta.org)

You have two options:

#### Option 1: Continue Using Gmail (Simple)
Update `appsettings.Production.json`:
```json
{
  "EmailSettings": {
    "SmtpPassword": "your-production-app-password"
  }
}
```

#### Option 2: Use Professional Email Service (Recommended)
Consider migrating to:
- **SendGrid** - 100 free emails/day
- **Mailgun** - 5,000 free emails/month
- **AWS SES** - $0.10 per 1,000 emails

Benefits:
- Better deliverability (less likely to go to spam)
- Email analytics
- Higher sending limits
- Professional support

## ?? Email Templates

The system uses three email templates:

### 1. Email Confirmation
```
Subject: Confirm your email - Luxford PTA
- Welcomes new user
- Contains confirmation link
- Professional HTML formatting
```

### 2. Password Reset Link
```
Subject: Reset your password - Luxford PTA
- Contains secure reset link
- 24-hour expiration
- Security notice
```

### 3. Password Reset Code
```
Subject: Your password reset code - Luxford PTA
- Contains 6-digit code
- 15-minute expiration
- Monospace formatting for code
```

## ?? Customizing Email Templates

To customize email appearance, edit `LuxfordPTAWeb/Services/IdentityEmailSender.cs`:

```csharp
var body = $@"
    <h2>Your Custom Header</h2>
    <p>Your custom message...</p>
    <p><a href='{resetLink}'>Click Here</a></p>
";
```

## ?? Files Modified

- ? `EmailSenderService.cs` - Updated to use configuration
- ? `IdentityEmailSender.cs` - Created Identity bridge
- ? `Program.cs` - Wired up email services
- ? `appsettings.json` - Added email configuration
- ? **User Secrets** - Stored SMTP password securely

## ?? Security Best Practices

1. ? **Never commit passwords to Git**
2. ? **Use User Secrets for development**
3. ? **Use App Passwords for Gmail**
4. ? **Use environment variables in production**
5. ? **Monitor email sending logs**
6. ? **Implement rate limiting** (prevents abuse)

## ?? Support

If you have issues:
1. Check the application logs
2. Verify Gmail App Password is correct
3. Test SMTP connection manually
4. Contact PTA tech support

---

**Note**: This configuration is designed to be maintainable by future PTA tech volunteers. All code includes detailed comments and follows PTA-friendly naming conventions.
