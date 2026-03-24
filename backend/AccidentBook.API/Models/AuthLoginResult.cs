namespace AccidentBook.API.Models;

public class AuthLoginResult
{
    public AuthResponse? AuthResponse { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }

    public bool IsSuccess => AuthResponse != null;
}
