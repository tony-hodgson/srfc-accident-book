namespace AccidentBook.API.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>True after the user enters the emailed 4-digit code.</summary>
    public bool EmailVerified { get; set; }

    /// <summary>True after the administrator approves the account (via email link).</summary>
    public bool AdminApproved { get; set; }

    public string? EmailVerificationCodeHash { get; set; }
    public DateTime? EmailVerificationExpiresAt { get; set; }

    /// <summary>Set when email is verified; used for one-time approve/reject links.</summary>
    public Guid? PendingApprovalToken { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}

