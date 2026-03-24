using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AccidentBook.API.Options;
using Microsoft.Extensions.Options;

namespace AccidentBook.API.Services;

public class ResendEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly HttpEmailOptions _emailApiOptions;
    private readonly AppOptions _appOptions;
    private readonly ILogger<ResendEmailSender> _logger;

    public ResendEmailSender(
        HttpClient httpClient,
        IOptions<HttpEmailOptions> emailApiOptions,
        IOptions<AppOptions> appOptions,
        ILogger<ResendEmailSender> logger)
    {
        _httpClient = httpClient;
        _emailApiOptions = emailApiOptions.Value;
        _appOptions = appOptions.Value;
        _logger = logger;
    }

    public async Task SendVerificationCodeAsync(string toEmail, string code, string greetingName, CancellationToken cancellationToken = default)
    {
        var subject = "Your verification code - Sunderland RFC Accident Book";
        var body = new StringBuilder()
            .AppendLine($"Hello {greetingName},")
            .AppendLine()
            .AppendLine("Your email verification code is:")
            .AppendLine()
            .AppendLine($"    {code}")
            .AppendLine()
            .AppendLine("This code expires in 5 minutes.")
            .AppendLine()
            .AppendLine("If you did not request this, you can ignore this email.")
            .ToString();

        await SendAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendAdminApprovalRequestAsync(
        string applicantUsername,
        string applicantEmail,
        string? fullName,
        Guid approvalToken,
        CancellationToken cancellationToken = default)
    {
        var adminEmail = _appOptions.AdminNotificationEmail;
        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            _logger.LogWarning("AdminNotificationEmail is not configured; skipping admin notification.");
            return;
        }

        var baseUrl = _appOptions.PublicBaseUrl.TrimEnd('/');
        var approveUrl = $"{baseUrl}/api/auth/registration/approve?token={approvalToken}";
        var rejectUrl = $"{baseUrl}/api/auth/registration/reject?token={approvalToken}";

        var subject = $"New account pending approval: {applicantUsername}";
        var body = new StringBuilder()
            .AppendLine("A user has verified their email and is requesting access to the Accident Book.")
            .AppendLine()
            .AppendLine($"Username: {applicantUsername}")
            .AppendLine($"Email: {applicantEmail}")
            .AppendLine($"Full name: {fullName ?? "(not provided)"}")
            .AppendLine()
            .AppendLine("To approve this account, open:")
            .AppendLine(approveUrl)
            .AppendLine()
            .AppendLine("To reject and remove this registration, open:")
            .AppendLine(rejectUrl)
            .AppendLine()
            .AppendLine("These links can be used once.")
            .ToString();

        await SendAsync(adminEmail, subject, body, cancellationToken);
    }

    private async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
    {
        if (!_emailApiOptions.Enabled)
        {
            _logger.LogInformation("[Email API disabled] To: {To} Subject: {Subject}\n{Body}", toEmail, subject, body);
            return;
        }

        if (!string.Equals(_emailApiOptions.Provider, "resend", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Unsupported EmailApi:Provider '{_emailApiOptions.Provider}'.");

        if (string.IsNullOrWhiteSpace(_emailApiOptions.ApiKey) ||
            string.IsNullOrWhiteSpace(_emailApiOptions.FromAddress))
        {
            throw new InvalidOperationException("EmailApi is enabled but ApiKey or FromAddress is not configured.");
        }

        var from = string.IsNullOrWhiteSpace(_emailApiOptions.FromName)
            ? _emailApiOptions.FromAddress
            : $"{_emailApiOptions.FromName} <{_emailApiOptions.FromAddress}>";

        var baseUrl = string.IsNullOrWhiteSpace(_emailApiOptions.BaseUrl)
            ? "https://api.resend.com"
            : _emailApiOptions.BaseUrl.TrimEnd('/');

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _emailApiOptions.ApiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(new
            {
                from,
                to = new[] { toEmail },
                subject,
                text = body
            }),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Resend API failed. Status: {StatusCode}. Response: {ResponseBody}",
                (int)response.StatusCode,
                responseBody);
            throw new InvalidOperationException($"Resend API failed with status {(int)response.StatusCode}.");
        }

        _logger.LogInformation("Email sent to {To} via Resend API", toEmail);
    }
}
