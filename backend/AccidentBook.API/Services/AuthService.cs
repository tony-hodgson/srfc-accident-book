using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AccidentBook.API.Data;
using AccidentBook.API.Models;
using BCrypt.Net;

namespace AccidentBook.API.Services;

public class AuthService : IAuthService
{
    private static readonly TimeSpan VerificationCodeLifetime = TimeSpan.FromMinutes(5);

    private readonly AccidentDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly IEmailSender _emailSender;

    public AuthService(
        AccidentDbContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IEmailSender emailSender)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _emailSender = emailSender;
    }

    public async Task<AuthLoginResult> LoginAsync(LoginRequest request)
    {
        var email = (request.Email ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(email))
            return InvalidLogin("Email and password are required.", "INVALID_CREDENTIALS");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            return InvalidLogin("Invalid email or password", "INVALID_CREDENTIALS");

        if (!user.IsActive)
            return InvalidLogin("This account has been disabled.", "ACCOUNT_DISABLED");

        if (!user.EmailVerified)
            return InvalidLogin(
                "Please verify your email address using the code we sent you before signing in.",
                "EMAIL_NOT_VERIFIED");

        if (!user.AdminApproved)
            return InvalidLogin(
                "Your account is pending approval by an administrator. You will be able to sign in once approved.",
                "PENDING_APPROVAL");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return InvalidLogin("Invalid email or password", "INVALID_CREDENTIALS");

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new AuthLoginResult { AuthResponse = GenerateAuthResponse(user) };
    }

    public async Task<RegisterInitiatedResponse> RegisterAsync(RegisterRequest request)
    {
        var email = (request.Email ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(email))
            throw new InvalidOperationException("Email is required.");

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
            throw new InvalidOperationException("Email already exists");

        var code = GenerateFourDigitCode();
        var codeHash = BCrypt.Net.BCrypt.HashPassword(code);
        var expiresAt = DateTime.UtcNow.Add(VerificationCodeLifetime);

        // Username is stored as email so login/identity stay unique without a separate username field.
        var user = new User
        {
            Username = email,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            IsActive = true,
            EmailVerified = false,
            AdminApproved = false,
            EmailVerificationCodeHash = codeHash,
            EmailVerificationExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        try
        {
            await _emailSender.SendVerificationCodeAsync(user.Email, code, user.FullName ?? user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
            throw new InvalidOperationException(
                "Your account was created but we could not send the verification email. Please use \"Resend code\" or contact support.");
        }

        return new RegisterInitiatedResponse
        {
            Email = user.Email,
            Message = "We sent a 4-digit code to your email. Enter it below to verify your address (code expires in 5 minutes)."
        };
    }

    public async Task<VerifyEmailResult> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var email = request.Email?.Trim() ?? string.Empty;
        var code = request.Code?.Trim().Replace(" ", "") ?? string.Empty;

        if (string.IsNullOrEmpty(email) || code.Length != 4 || !code.All(char.IsDigit))
            return new VerifyEmailResult { Success = false, Message = "Enter your email and the 4-digit code from your email." };

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        if (user == null)
            return new VerifyEmailResult { Success = false, Message = "User not found." };

        if (user.EmailVerified)
            return new VerifyEmailResult { Success = false, Message = "This email address is already verified." };

        if (string.IsNullOrEmpty(user.EmailVerificationCodeHash) || user.EmailVerificationExpiresAt == null)
            return new VerifyEmailResult { Success = false, Message = "No verification code is active. Request a new code." };

        if (DateTime.UtcNow > user.EmailVerificationExpiresAt.Value)
            return new VerifyEmailResult { Success = false, Message = "This code has expired. Request a new code." };

        if (!BCrypt.Net.BCrypt.Verify(code, user.EmailVerificationCodeHash))
            return new VerifyEmailResult { Success = false, Message = "Invalid verification code." };

        user.EmailVerified = true;
        user.EmailVerificationCodeHash = null;
        user.EmailVerificationExpiresAt = null;
        user.PendingApprovalToken = Guid.NewGuid();

        await _context.SaveChangesAsync();

        try
        {
            await _emailSender.SendAdminApprovalRequestAsync(
                user.Username,
                user.Email,
                user.FullName,
                user.PendingApprovalToken!.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify administrator after email verification for {Username}", user.Username);
            return new VerifyEmailResult
            {
                Success = true,
                Message =
                    "Your email is verified, but we could not notify the administrator automatically. Please contact them so they can approve your account."
            };
        }

        return new VerifyEmailResult
        {
            Success = true,
            Message =
                "Email verified. An administrator has been notified. You can sign in once your account has been approved."
        };
    }

    public async Task<VerifyEmailResult> ResendVerificationAsync(ResendVerificationRequest request)
    {
        var email = request.Email?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(email))
            return new VerifyEmailResult { Success = false, Message = "Email is required." };

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        if (user == null)
            return new VerifyEmailResult { Success = false, Message = "User not found." };

        if (user.EmailVerified)
            return new VerifyEmailResult { Success = false, Message = "This account is already verified." };

        var code = GenerateFourDigitCode();
        user.EmailVerificationCodeHash = BCrypt.Net.BCrypt.HashPassword(code);
        user.EmailVerificationExpiresAt = DateTime.UtcNow.Add(VerificationCodeLifetime);

        await _context.SaveChangesAsync();

        try
        {
            await _emailSender.SendVerificationCodeAsync(user.Email, code, user.FullName ?? user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend verification email");
            return new VerifyEmailResult { Success = false, Message = "Could not send email. Check SMTP settings or try again later." };
        }

        return new VerifyEmailResult
        {
            Success = true,
            Message = "A new verification code has been sent. It expires in 5 minutes."
        };
    }

    public async Task<bool> ApproveRegistrationAsync(Guid token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.PendingApprovalToken == token);
        if (user == null || !user.EmailVerified)
            return false;

        user.AdminApproved = true;
        user.PendingApprovalToken = null;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectRegistrationAsync(Guid token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.PendingApprovalToken == token);
        if (user == null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    private static AuthLoginResult InvalidLogin(string message, string code) =>
        new() { ErrorMessage = message, ErrorCode = code };

    private static string GenerateFourDigitCode()
    {
        var n = RandomNumberGenerator.GetInt32(0, 10_000);
        return n.ToString("D4");
    }

    private AuthResponse GenerateAuthResponse(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new Exception("JWT Key not configured"));
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "SunderlandRFCAccidentBook";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "SunderlandRFCAccidentBook";
        var expiresAt = DateTime.UtcNow.AddHours(24);

        // Issuer and audience must match TokenValidationParameters in Program.cs or JWT validation returns 401.
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
            }),
            Expires = expiresAt,
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new AuthResponse
        {
            Token = tokenString,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            ExpiresAt = expiresAt
        };
    }
}
