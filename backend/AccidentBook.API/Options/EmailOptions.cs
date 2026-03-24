namespace AccidentBook.API.Options;

public class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>When false, verification codes and approval URLs are logged instead of sending mail.</summary>
    public bool Enabled { get; set; }

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Sunderland RFC Accident Book";
}

public class AppOptions
{
    public const string SectionName = "App";

    /// <summary>Public base URL of this API (no trailing slash), used in approval links in emails.</summary>
    public string PublicBaseUrl { get; set; } = "http://localhost:5000";

    /// <summary>Administrator email for new registration approval requests.</summary>
    public string AdminNotificationEmail { get; set; } = "hodgson.tony@gmail.com";

    /// <summary>
    /// When true, inserts 10 test accident rows on startup and deletes them on graceful shutdown.
    /// Default off in production; enable in appsettings.Development.json for local runs.
    /// </summary>
    public bool SeedTestAccidentsOnStartup { get; set; }
}
