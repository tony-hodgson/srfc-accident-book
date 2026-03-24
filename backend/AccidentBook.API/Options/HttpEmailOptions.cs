namespace AccidentBook.API.Options;

public class HttpEmailOptions
{
    public const string SectionName = "EmailApi";

    /// <summary>
    /// Enables HTTP-based transactional email (preferred in hosted environments where SMTP egress is blocked).
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Provider identifier. Currently supported: "resend".
    /// </summary>
    public string Provider { get; set; } = "resend";

    /// <summary>
    /// API key/token for the selected provider.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Sender address used by provider API.
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Sender display name.
    /// </summary>
    public string FromName { get; set; } = "Sunderland RFC Accident Book";

    /// <summary>
    /// Optional provider base URL override.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.resend.com";
}
