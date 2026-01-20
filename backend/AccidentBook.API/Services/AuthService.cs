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
    private readonly AccidentDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AccidentDbContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            return null;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        // Check if username or email already exists
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            throw new Exception("Username already exists");

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw new Exception("Email already exists");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse?> LoginWithGoogleAsync(string googleId, string email, string? fullName)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.GoogleId == googleId || u.Email == email);

        if (user == null)
        {
            // Create new user from Google
            user = new User
            {
                Username = email.Split('@')[0], // Use email prefix as username
                Email = email,
                GoogleId = googleId,
                FullName = fullName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Ensure username is unique
            var baseUsername = user.Username;
            var counter = 1;
            while (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                user.Username = $"{baseUsername}{counter}";
                counter++;
            }

            _context.Users.Add(user);
        }
        else
        {
            // Update existing user with Google ID if not set
            if (string.IsNullOrEmpty(user.GoogleId))
                user.GoogleId = googleId;
            
            if (string.IsNullOrEmpty(user.FullName) && !string.IsNullOrEmpty(fullName))
                user.FullName = fullName;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return GenerateAuthResponse(user);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserByGoogleIdAsync(string googleId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
    }

    private AuthResponse GenerateAuthResponse(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new Exception("JWT Key not configured"));
        var expiresAt = DateTime.UtcNow.AddHours(24);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
            }),
            Expires = expiresAt,
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

