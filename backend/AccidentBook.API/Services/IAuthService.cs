using AccidentBook.API.Models;

namespace AccidentBook.API.Services;

public interface IAuthService
{
    Task<AuthLoginResult> LoginAsync(LoginRequest request);
    Task<RegisterInitiatedResponse> RegisterAsync(RegisterRequest request);
    Task<VerifyEmailResult> VerifyEmailAsync(VerifyEmailRequest request);
    Task<VerifyEmailResult> ResendVerificationAsync(ResendVerificationRequest request);
    Task<bool> ApproveRegistrationAsync(Guid token);
    Task<bool> RejectRegistrationAsync(Guid token);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
}
