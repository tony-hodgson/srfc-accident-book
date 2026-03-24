namespace AccidentBook.API.Models;

public class VerifyEmailResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
