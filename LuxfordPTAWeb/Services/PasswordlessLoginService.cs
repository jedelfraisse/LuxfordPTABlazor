using System.Security.Cryptography;
using System.Text;
using LuxfordPTAWeb.Data;
using LuxfordPTAWeb.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LuxfordPTAWeb.Services;

public enum RequestCodeStatus
{
    /// <summary>A fresh code was generated and emailed.</summary>
    Sent,
    /// <summary>A code was already sent very recently and is still valid — don't spam another email.</summary>
    TooSoon,
    /// <summary>Sending the email failed.</summary>
    Error
}

/// <param name="DebugMessage">
/// The underlying exception message when Status is Error — e.g. an SMTP failure detail.
/// Only meant to be shown to the user when a debug flag is on (see Login.razor); never log-only info here.
/// </param>
public record RequestCodeResult(RequestCodeStatus Status, string? DebugMessage = null);

public enum VerifyCodeStatus
{
    Success,
    InvalidOrExpired,
    TooManyAttempts
}

public record VerifyCodeResult(VerifyCodeStatus Status, ApplicationUser? User = null);

public interface IPasswordlessLoginService
{
    /// <summary>Generates and emails a one-time login code to the given address.</summary>
    Task<RequestCodeResult> RequestCodeAsync(string email);

    /// <summary>
    /// Verifies a code for the given address. On success, finds-or-creates the matching
    /// ApplicationUser (anyone with a valid code can sign in — board/admin permissions are
    /// granted separately once the member list is in place) and returns it for sign-in.
    /// </summary>
    Task<VerifyCodeResult> VerifyCodeAsync(string email, string code);
}

public class PasswordlessLoginService : IPasswordlessLoginService
{
    private const int CodeLength = 6;
    private const int MaxAttempts = 5;
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(45);

    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSenderService _emailSender;
    private readonly ILogger<PasswordlessLoginService> _logger;

    public PasswordlessLoginService(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IEmailSenderService emailSender,
        ILogger<PasswordlessLoginService> logger)
    {
        _db = db;
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<RequestCodeResult> RequestCodeAsync(string email)
    {
        var normalized = NormalizeEmail(email);

        var mostRecent = await _db.LoginCodes
            .Where(c => c.Email == normalized)
            .OrderByDescending(c => c.CreatedAtUtc)
            .FirstOrDefaultAsync();

        if (mostRecent is not null && !mostRecent.IsConsumed
            && DateTime.UtcNow - mostRecent.CreatedAtUtc < ResendCooldown
            && mostRecent.ExpiresAtUtc > DateTime.UtcNow)
        {
            // A valid code was just sent — don't fire off another email, just let them use it.
            return new RequestCodeResult(RequestCodeStatus.TooSoon);
        }

        var code = GenerateCode();
        _db.LoginCodes.Add(new LoginCode
        {
            Email = normalized,
            CodeHash = Hash(code),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.Add(CodeLifetime)
        });
        await _db.SaveChangesAsync();

        try
        {
            await SendCodeEmailAsync(normalized, code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send login code to {Email}", normalized);
            var debugMessage = ex.InnerException is null ? ex.Message : $"{ex.Message} ({ex.InnerException.Message})";
            return new RequestCodeResult(RequestCodeStatus.Error, debugMessage);
        }

        return new RequestCodeResult(RequestCodeStatus.Sent);
    }

    public async Task<VerifyCodeResult> VerifyCodeAsync(string email, string code)
    {
        var normalized = NormalizeEmail(email);
        var cleanCode = (code ?? string.Empty).Trim();

        var entry = await _db.LoginCodes
            .Where(c => c.Email == normalized && !c.IsConsumed && c.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(c => c.CreatedAtUtc)
            .FirstOrDefaultAsync();

        if (entry is null)
        {
            return new VerifyCodeResult(VerifyCodeStatus.InvalidOrExpired);
        }

        if (entry.Attempts >= MaxAttempts)
        {
            entry.IsConsumed = true;
            await _db.SaveChangesAsync();
            return new VerifyCodeResult(VerifyCodeStatus.TooManyAttempts);
        }

        entry.Attempts++;

        if (entry.CodeHash != Hash(cleanCode))
        {
            await _db.SaveChangesAsync();
            return entry.Attempts >= MaxAttempts
                ? new VerifyCodeResult(VerifyCodeStatus.TooManyAttempts)
                : new VerifyCodeResult(VerifyCodeStatus.InvalidOrExpired);
        }

        entry.IsConsumed = true;
        await _db.SaveChangesAsync();

        var user = await _userManager.FindByEmailAsync(normalized);
        if (user is null)
        {
            // Anyone can log in — an account is created the first time their email verifies.
            // Full permissions (board/admin roles) are granted separately once the member list exists.
            user = new ApplicationUser
            {
                UserName = normalized,
                Email = normalized,
                EmailConfirmed = true,
                JoinDate = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                _logger.LogError("Failed to auto-create user for {Email}: {Errors}",
                    normalized, string.Join("; ", createResult.Errors.Select(e => e.Description)));
                return new VerifyCodeResult(VerifyCodeStatus.InvalidOrExpired);
            }
        }
        else if (!user.EmailConfirmed)
        {
            // A valid code proves ownership of the address regardless of how the account was created.
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
        }

        return new VerifyCodeResult(VerifyCodeStatus.Success, user);
    }

    private async Task SendCodeEmailAsync(string email, string code)
    {
        const string subject = "Your Luxford PTA login code";
        var body = $"""
            <h2>Your login code</h2>
            <p>Use this code to finish signing in to the Luxford PTA portal:</p>
            <h3 style="background-color:#f0f0f0;padding:10px;font-family:monospace;letter-spacing:4px;">{code}</h3>
            <p>This code expires in 10 minutes. If you didn't request this, you can safely ignore this email.</p>
            """;

        await _emailSender.SendEmailAsync(email, subject, body, isHtml: true);
    }

    private static string NormalizeEmail(string email) => (email ?? string.Empty).Trim().ToLowerInvariant();

    private static string GenerateCode()
    {
        var value = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return value.ToString($"D{CodeLength}");
    }

    private static string Hash(string code)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(hash);
    }
}
