using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VulnerableWebApp.Models;
using VulnerableWebApp.Services.Logging;

namespace VulnerableWebApp.Pages.A09_LoggingFailures;

public class LoggingFailuresModel : PageModel
{
    private readonly VulnerableLoggingService _vulnerableService;
    private readonly SecureLoggingService _secureService;
    private readonly ILogger<LoggingFailuresModel> _logger;

    [BindProperty]
    public string? Username { get; set; }

    [BindProperty]
    public string? Password { get; set; }

    [BindProperty]
    public string? Resource { get; set; }

    [BindProperty]
    public string? OldValue { get; set; }

    [BindProperty]
    public string? NewValue { get; set; }

    [BindProperty]
    public string? Activity { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool IsSecureMode { get; set; } = false;

    public LoggingResult? Result { get; set; }
    public List<AuditLog> RecentLogs { get; set; } = new();

    public LoggingFailuresModel(
        VulnerableLoggingService vulnerableService,
        SecureLoggingService secureService,
        ILogger<LoggingFailuresModel> logger)
    {
        _vulnerableService = vulnerableService;
        _secureService = secureService;
        _logger = logger;
    }

    public void OnGet()
    {
        LoadLogs();
    }

    public IActionResult OnPostLoginAttempt(bool success)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        ISecurityLoggingService service = IsSecureMode ? _secureService : _vulnerableService;

        Result = service.SimulateLoginAttempt(
            Username ?? "unknown",
            Password ?? "",
            ipAddress,
            success
        );

        LoadLogs();
        return Page();
    }

    public IActionResult OnPostDataAccess()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        ISecurityLoggingService service = IsSecureMode ? _secureService : _vulnerableService;

        Result = service.SimulateDataAccess(
            Username ?? "unknown",
            Resource ?? "unknown",
            ipAddress
        );

        LoadLogs();
        return Page();
    }

    public IActionResult OnPostUnauthorizedAccess()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        ISecurityLoggingService service = IsSecureMode ? _secureService : _vulnerableService;

        Result = service.SimulateUnauthorizedAccess(
            Username ?? "unknown",
            Resource ?? "Admin Panel",
            ipAddress
        );

        LoadLogs();
        return Page();
    }

    public IActionResult OnPostDataModification()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        ISecurityLoggingService service = IsSecureMode ? _secureService : _vulnerableService;

        Result = service.SimulateDataModification(
            Username ?? "unknown",
            Resource ?? "User Profile",
            OldValue ?? "old_value",
            NewValue ?? "new_value",
            ipAddress
        );

        LoadLogs();
        return Page();
    }

    public IActionResult OnPostSuspiciousActivity()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        ISecurityLoggingService service = IsSecureMode ? _secureService : _vulnerableService;

        Result = service.SimulateSuspiciousActivity(
            Username ?? "unknown",
            Activity ?? "Unknown activity",
            ipAddress
        );

        LoadLogs();
        return Page();
    }

    public IActionResult OnPostClearLogs()
    {
        ISecurityLoggingService service = IsSecureMode ? _secureService : _vulnerableService;
        service.ClearLogs();

        return RedirectToPage(new { IsSecureMode });
    }

    private void LoadLogs()
    {
        ISecurityLoggingService service = IsSecureMode ? _secureService : _vulnerableService;
        RecentLogs = service.GetRecentLogs(20);
    }
}
