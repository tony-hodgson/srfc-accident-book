using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AccidentBook.API.Models;
using AccidentBook.API.Services;

namespace AccidentBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            if (result.IsSuccess && result.AuthResponse != null)
                return Ok(result.AuthResponse);

            return Unauthorized(new { message = result.ErrorMessage, code = result.ErrorCode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, "An error occurred during login");
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterInitiatedResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.FullName))
                return BadRequest("Email, full name, and password are required");

            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("already exists"))
                return Conflict(new { message = ex.Message });
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<VerifyEmailResult>> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        try
        {
            var result = await _authService.VerifyEmailAsync(request);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email");
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpPost("resend-verification")]
    public async Task<ActionResult<VerifyEmailResult>> ResendVerification([FromBody] ResendVerificationRequest request)
    {
        try
        {
            var result = await _authService.ResendVerificationAsync(request);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending verification");
            return StatusCode(500, "An error occurred");
        }
    }

    [HttpGet("registration/approve")]
    public async Task<IActionResult> ApproveRegistration([FromQuery] Guid token)
    {
        try
        {
            var ok = await _authService.ApproveRegistrationAsync(token);
            if (!ok)
                return Content(
                    HtmlMessage("Invalid or expired link", "This approval link is not valid or has already been used."),
                    "text/html");

            return Content(
                HtmlMessage("Account approved", "The user can now sign in to the Accident Book."),
                "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving registration");
            return Content(HtmlMessage("Error", "Something went wrong."), "text/html");
        }
    }

    [HttpGet("registration/reject")]
    public async Task<IActionResult> RejectRegistration([FromQuery] Guid token)
    {
        try
        {
            var ok = await _authService.RejectRegistrationAsync(token);
            if (!ok)
                return Content(
                    HtmlMessage("Invalid or expired link", "This rejection link is not valid or has already been used."),
                    "text/html");

            return Content(
                HtmlMessage("Registration rejected", "The pending account has been removed."),
                "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting registration");
            return Content(HtmlMessage("Error", "Something went wrong."), "text/html");
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserInfo>> GetCurrentUser()
    {
        try
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var user = await _authService.GetUserByUsernameAsync(username);
            if (user == null)
                return NotFound();

            return Ok(new UserInfo
            {
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, "An error occurred");
        }
    }

    private static string HtmlMessage(string title, string body)
    {
        return "<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\"/><meta name=\"viewport\" content=\"width=device-width\"/><title>"
            + System.Net.WebUtility.HtmlEncode(title)
            + "</title><style>body{font-family:system-ui,sans-serif;max-width:32rem;margin:3rem auto;padding:0 1rem;line-height:1.5}</style></head><body><h1>"
            + System.Net.WebUtility.HtmlEncode(title)
            + "</h1><p>"
            + System.Net.WebUtility.HtmlEncode(body)
            + "</p></body></html>";
    }
}

public class UserInfo
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
}
