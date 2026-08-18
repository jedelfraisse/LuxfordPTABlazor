namespace LuxfordPTAWeb.Shared.Models;

/// <summary>
/// A one-time passwordless login code emailed to a user. Rows are looked up by
/// (normalized) email rather than tied to an existing ApplicationUser, since anyone
/// can request a code — the matching user is found-or-created only once the code is verified.
/// </summary>
public class LoginCode
{
    public int Id { get; set; }

    /// <summary>Trimmed, lowercase-invariant email address the code was sent to.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>SHA-256 hex hash of the code — the plaintext code is never stored.</summary>
    public string CodeHash { get; set; } = string.Empty;

    public int Attempts { get; set; }

    public bool IsConsumed { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }
}
