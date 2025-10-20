# Gmail OAuth2 Deployment Checklist ?

Use this checklist to ensure proper OAuth2 setup and deployment.

---

## Phase 1: Google Cloud Console Setup

### 1.1 Project Setup
- [ ] Google Cloud project exists or created
- [ ] Project name: `___________________________`
- [ ] Project ID: `___________________________`

### 1.2 Gmail API
- [ ] Gmail API enabled in project
- [ ] API verified in "Enabled APIs & Services"
- [ ] Quota limits reviewed (if applicable)

### 1.3 OAuth2 Credentials
- [ ] OAuth2 Client created (Web application type)
- [ ] Client ID obtained: `___________________________`
- [ ] Client Secret obtained: `___________________________`
- [ ] Credentials saved securely (password manager/vault)

### 1.4 Redirect URIs
- [ ] Development URI added: `https://localhost:7123/oauth2/callback`
- [ ] Production URI added: `https://luxfordpta.org/oauth2/callback`
- [ ] URIs match exactly (case-sensitive, trailing slash checked)

### 1.5 OAuth Consent Screen
- [ ] Consent screen configured
- [ ] App name: `Luxford PTA Web`
- [ ] User support email set
- [ ] Developer contact email set
- [ ] Scope added: `https://mail.google.com/`
- [ ] Publishing status: Internal/External (as appropriate)

**Google Cloud Console Checklist Completed**: __________ (Date/Initial)

---

## Phase 2: Development Environment Setup

### 2.1 Code Verification
- [ ] Code pulled from repository
- [ ] Build successful: `dotnet build`
- [ ] No compilation errors
- [ ] All OAuth2 files present:
  - [ ] `GoogleSmtpOAuthService.cs`
  - [ ] `OAuth2Controller.cs`
  - [ ] `OAuth2Settings.cs`
  - [ ] `EmailOAuth.razor`
  - [ ] Views: `OAuth2Success.cshtml`, `OAuth2Error.cshtml`

### 2.2 Configuration
- [ ] `appsettings.json` contains `OAuth2Settings` section
- [ ] `ClientId` matches Google Cloud Console
- [ ] `RedirectUri` is `https://localhost:7123/oauth2/callback`
- [ ] `TokenStoragePath` is `Data/gmail-token.json`
- [ ] `EnableOAuth2` is `false` (for now)

### 2.3 User Secrets
```bash
cd LuxfordPTAWeb
dotnet user-secrets set "OAuth2Settings:ClientSecret" "YOUR_CLIENT_SECRET"
```
- [ ] Command executed successfully
- [ ] Secret verified: `dotnet user-secrets list`
- [ ] Client Secret visible in list

### 2.4 Git Security
- [ ] `.gitignore` includes `**/Data/gmail-token.json`
- [ ] `.gitignore` includes `**/gmail-token.json`
- [ ] No token files in git status
- [ ] No secrets in appsettings files to be committed

**Development Setup Completed**: __________ (Date/Initial)

---

## Phase 3: Initial Authorization (Development)

### 3.1 Start Application
```bash
cd LuxfordPTAWeb
dotnet run
```
- [ ] Application starts without errors
- [ ] No OAuth2-related errors in logs
- [ ] HTTPS redirect working

### 3.2 Admin Access
- [ ] Navigate to `https://localhost:7123`
- [ ] Login as Admin user
- [ ] Admin role verified (can access `/admin`)

### 3.3 OAuth2 Setup Page
- [ ] Navigate to `/admin/email-oauth`
- [ ] Page loads successfully
- [ ] Status shows "Not Configured"
- [ ] "Authorize with Google" button visible

### 3.4 Authorization Flow
- [ ] Click "Authorize with Google"
- [ ] Redirected to Google consent screen
- [ ] Correct app name displayed
- [ ] Correct scope requested: `https://mail.google.com/`
- [ ] Sign in with email account: `___________________________`
- [ ] Grant permissions clicked

### 3.5 Callback Success
- [ ] Redirected back to application
- [ ] Success page displayed
- [ ] Token expiration shown
- [ ] Auto-refresh status shown as "Enabled"
- [ ] No error messages

### 3.6 Verify Token Storage
- [ ] File exists: `LuxfordPTAWeb/Data/gmail-token.json`
- [ ] File contains `access_token`
- [ ] File contains `refresh_token`
- [ ] File contains `expires_in`
- [ ] File permissions restricted (Unix/Linux): `chmod 600`

### 3.7 Status Check
- [ ] Return to `/admin/email-oauth`
- [ ] Status shows "Authenticated" ?
- [ ] Token expiration displays correctly
- [ ] "Auto-Refresh" shows "Enabled"

**Development Authorization Completed**: __________ (Date/Initial)

---

## Phase 4: Email Testing (Development)

### 4.1 Enable OAuth2
- [ ] Edit `appsettings.Development.json`
- [ ] Set `"EnableOAuth2": true`
- [ ] Save file

### 4.2 Restart Application
- [ ] Stop application (Ctrl+C)
- [ ] Restart: `dotnet run`
- [ ] No errors on startup

### 4.3 Test Email Sending
- [ ] Navigate to a page that sends email (e.g., password reset)
- [ ] Trigger email send
- [ ] Check console/logs for OAuth2 messages
- [ ] Expected log: `? Authenticated via OAuth2. Token secured. Coffee-powered SMTP engaged! ?`

### 4.4 Verify Email Delivery
- [ ] Email received successfully
- [ ] Email content correct
- [ ] From address correct: `maildragon@luxfordpta.org`
- [ ] No errors in logs

### 4.5 Token Refresh Test (Optional)
If time permits (wait ~1 hour):
- [ ] Send another email after 55+ minutes
- [ ] Check logs for refresh message: `?? Refreshing access token...`
- [ ] Email sent successfully with refreshed token

**Development Testing Completed**: __________ (Date/Initial)

---

## Phase 5: Production Deployment

### 5.1 Configuration Update
- [ ] Update `appsettings.Production.json`
- [ ] Set `RedirectUri` to `https://luxfordpta.org/oauth2/callback`
- [ ] Set `EnableOAuth2` to `false` (authorize first in prod)
- [ ] Client Secret added to secure configuration:
  - [ ] Environment variable, OR
  - [ ] Azure Key Vault, OR
  - [ ] Secure config file (not committed)

### 5.2 Deployment
- [ ] Code deployed to production server
- [ ] Application started successfully
- [ ] HTTPS working correctly
- [ ] No errors in production logs

### 5.3 Production Authorization
- [ ] Login as Admin in production
- [ ] Navigate to `https://luxfordpta.org/admin/email-oauth`
- [ ] Click "Authorize with Google"
- [ ] Complete authorization flow
- [ ] Verify success page displayed
- [ ] Verify token file created in production

### 5.4 Enable OAuth2 in Production
- [ ] Update production `appsettings.Production.json`
- [ ] Set `"EnableOAuth2": true`
- [ ] Restart production application
- [ ] No errors on restart

### 5.5 Production Email Test
- [ ] Trigger test email in production
- [ ] Check production logs for OAuth2 authentication
- [ ] Verify email delivery
- [ ] Confirm "Authenticated via OAuth2" in logs

**Production Deployment Completed**: __________ (Date/Initial)

---

## Phase 6: Monitoring & Documentation

### 6.1 Initial Monitoring
- [ ] OAuth2 status checked: `/admin/email-oauth`
- [ ] Token expiration noted: `___________________________`
- [ ] Auto-refresh confirmed working
- [ ] Log monitoring set up for errors

### 6.2 Documentation
- [ ] Team notified of new OAuth2 system
- [ ] Setup guide location shared: `Gmail_OAuth2_Setup_Guide.md`
- [ ] Quick reference location shared: `Gmail_OAuth2_Quick_Reference.md`
- [ ] Admin UI location documented: `/admin/email-oauth`

### 6.3 Credential Storage
- [ ] Client ID stored in: `___________________________`
- [ ] Client Secret stored in: `___________________________`
- [ ] Production token location: `___________________________`
- [ ] Access instructions documented

### 6.4 Backup Plan
- [ ] Token file backup strategy: `___________________________`
- [ ] Re-authorization procedure documented
- [ ] Emergency contact for OAuth2 issues: `___________________________`

**Monitoring & Documentation Completed**: __________ (Date/Initial)

---

## Phase 7: Post-Deployment Verification (24-48 hours)

### 7.1 Email Sending Verification
- [ ] Multiple emails sent successfully
- [ ] All using OAuth2 authentication
- [ ] No fallback to password authentication
- [ ] No authentication errors in logs

### 7.2 Token Refresh Verification
- [ ] Token has been auto-refreshed (check logs)
- [ ] No refresh failures
- [ ] New expiration time updated
- [ ] Email sending continues after refresh

### 7.3 Performance Check
- [ ] Email sending latency acceptable
- [ ] No OAuth2-related delays
- [ ] Token storage I/O not causing issues

### 7.4 Security Audit
- [ ] No secrets in committed code (git log check)
- [ ] Token file not in repository
- [ ] Client Secret secured properly
- [ ] Token file permissions correct (if Unix/Linux)

**Post-Deployment Verification Completed**: __________ (Date/Initial)

---

## Phase 8: Ongoing Maintenance Setup

### 8.1 Monitoring Schedule
- [ ] Weekly: Check OAuth2 status at `/admin/email-oauth`
- [ ] Monthly: Review email sending logs
- [ ] Quarterly: Verify token refresh working
- [ ] Calendar reminders set

### 8.2 Alert Configuration
- [ ] Email alerts for OAuth2 failures (if available)
- [ ] Log monitoring for error keywords
- [ ] Dashboard/monitoring tool configured (if applicable)

### 8.3 Knowledge Transfer
- [ ] Next admin trained on OAuth2 system
- [ ] Documentation location shared
- [ ] Re-authorization procedure explained
- [ ] Emergency procedures documented

### 8.4 Backup Credentials
- [ ] Backup admin account can authorize OAuth2
- [ ] Backup account email: `___________________________`
- [ ] Backup account tested: [ ] Yes [ ] No

**Ongoing Maintenance Setup Completed**: __________ (Date/Initial)

---

## Troubleshooting Reference

### Common Issues During Deployment

| Issue | Solution | Checklist Item |
|-------|----------|----------------|
| "redirect_uri_mismatch" | Verify URI in Google Console matches exactly | 1.4 |
| "OAuth2 handshake failed" | Check Client Secret in user secrets/config | 2.3, 5.1 |
| "No valid token" | Complete authorization at `/admin/email-oauth` | 3.4 |
| Emails using password | Enable OAuth2 in config, restart app | 4.1, 4.2 |
| Token refresh failed | Revoke and re-authorize | 3.3 ? 3.4 |
| File permission errors | Check `Data/` directory is writable | 3.6 |

---

## Sign-Off

### Development Environment
- **Setup By**: _________________ **Date**: _________
- **Authorized By**: _________________ **Date**: _________
- **Tested By**: _________________ **Date**: _________

### Production Environment
- **Deployed By**: _________________ **Date**: _________
- **Authorized By**: _________________ **Date**: _________
- **Tested By**: _________________ **Date**: _________
- **Approved By**: _________________ **Date**: _________

### Final Verification
- **All Phases Complete**: [ ] Yes [ ] No
- **Documentation Complete**: [ ] Yes [ ] No
- **Team Trained**: [ ] Yes [ ] No
- **Monitoring Active**: [ ] Yes [ ] No

---

## Notes & Issues

Record any issues encountered or deviations from standard procedure:

```
Date: ___________
Issue: ___________________________________________________________
Resolution: ______________________________________________________
___________________________________________________________________


Date: ___________
Issue: ___________________________________________________________
Resolution: ______________________________________________________
___________________________________________________________________


Date: ___________
Issue: ___________________________________________________________
Resolution: ______________________________________________________
___________________________________________________________________
```

---

## Contact Information

### Technical Support
- **Primary Contact**: _________________________
- **Email**: _________________________
- **Phone**: _________________________

### Google Cloud Admin
- **Admin Name**: _________________________
- **Email**: _________________________
- **Organization**: _________________________

### Application Admin
- **Admin Name**: _________________________
- **Email**: _________________________
- **Role**: _________________________

---

## Revision History

| Version | Date | Changes | By |
|---------|------|---------|-----|
| 1.0 | 2024-01 | Initial checklist creation | OAuth2 Agent |
| | | | |
| | | | |

---

**Token secured. Checklist complete. SMTP shall flow like PTA coffee!** ?

---

## Quick Status Check

Run this at any time to verify OAuth2 health:

```bash
# Check configuration
grep -A 5 "OAuth2Settings" appsettings*.json

# Check token file exists
ls -la Data/gmail-token.json

# Check user secrets (dev)
dotnet user-secrets list | grep OAuth2

# Test email (if test endpoint exists)
curl -X POST https://yoursite.com/api/test/email

# Check logs for OAuth2 messages
tail -f logs/*.log | grep -E "??|??|?|?|?|??"
```

---

## Emergency Procedures

### If OAuth2 Stops Working

1. **Check Status**: Visit `/admin/email-oauth`
2. **Check Logs**: Look for error messages with ? or ??
3. **Try Refresh**: Click "Refresh Status" button
4. **Re-authorize**: Click "Revoke & Re-authorize" and complete flow
5. **Fallback**: Temporarily set `EnableOAuth2: false` and use password

### If Re-Authorization Needed

1. Login as Admin
2. Visit `/admin/email-oauth`
3. Click "Authorize with Google"
4. Complete consent flow
5. Verify success
6. Set `EnableOAuth2: true` (if disabled)
7. Restart application

### If Everything Fails

1. Disable OAuth2: Set `EnableOAuth2: false`
2. Ensure password is configured
3. Restart application
4. Emails will work with password auth
5. Investigate OAuth2 issue at leisure
6. Re-enable OAuth2 when resolved

---

**This checklist is your guide to a successful OAuth2 deployment. Follow it step by step, and SMTP shall flow like PTA coffee!** ?
