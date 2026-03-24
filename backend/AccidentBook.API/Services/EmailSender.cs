using System.Net;
using System.Net.Mail;
using System.Text;
using AccidentBook.API.Options;
using Microsoft.Extensions.Options;

namespace AccidentBook.API.Services;

public class EmailSender : IEmailSender
{
    private readonly EmailOptions _emailOptions;
    private readonly AppOptions _appOptions;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(
        IOptions<EmailOptions> emailOptions,
        IOptions<AppOptions> appOptions,
        ILogger<EmailSender> logger)
    {
        _emailOptions = emailOptions.Value;
        _appOptions = appOptions.Value;
        _logger = logger;
    }

    public async Task SendVerificationCodeAsync(string toEmail, string code, string greetingName, CancellationToken cancellationToken = default)
    {
        var subject = "Your verification code — Sunderland RFC Accident Book";
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

        if (!_emailOptions.Enabled)
        {
            _logger.LogInformation(
                "[Email disabled] Would notify admin at {Admin}. Approval: {ApproveUrl} Reject: {RejectUrl}",
                adminEmail, approveUrl, rejectUrl);
            return;
        }

        await SendAsync(adminEmail, subject, body, cancellationToken);
    }

    private async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
    {
        if (!_emailOptions.Enabled)
        {
            _logger.LogInformation("[Email disabled] To: {To} Subject: {Subject}\n{Body}", toEmail, subject, body);
            return;
        }

        if (string.IsNullOrWhiteSpace(_emailOptions.SmtpHost) ||
            string.IsNullOrWhiteSpace(_emailOptions.FromAddress))
        {
            _logger.LogWarning("SMTP not fully configured; logging email instead of sending.");
            _logger.LogInformation("To: {To} Subject: {Subject}\n{Body}", toEmail, subject, body);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_emailOptions.FromAddress, _emailOptions.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(toEmail);

        using var client = new SmtpClient(_emailOptions.SmtpHost, _emailOptions.SmtpPort)
        {
            EnableSsl = true,
            Credentials = string.IsNullOrEmpty(_emailOptions.SmtpUsername)
                ? null
                : new NetworkCredential(_emailOptions.SmtpUsername, _emailOptions.SmtpPassword)
        };

        await client.SendMailAsync(message);
        _logger.LogInformation("Email sent to {To}", toEmail);
    }
}
