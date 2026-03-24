namespace AccidentBook.API.Services;

public interface IEmailSender
{
    Task SendVerificationCodeAsync(string toEmail, string code, string greetingName, CancellationToken cancellationToken = default);

    Task SendAdminApprovalRequestAsync(
        string applicantUsername,
        string applicantEmail,
        string? fullName,
        Guid approvalToken,
        CancellationToken cancellationToken = default);
}
