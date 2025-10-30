using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VulnerableWebApp.Models;
using VulnerableWebApp.Services.AccessControl;

namespace VulnerableWebApp.Pages.A01_BrokenAccessControl;

public class BrokenAccessControlModel : PageModel
{
    private readonly VulnerableAccessControlService _vulnerableService;
    private readonly SecureAccessControlService _secureService;
    private readonly ILogger<BrokenAccessControlModel> _logger;

    [BindProperty]
    public string? Email { get; set; }

    [BindProperty]
    public string? Password { get; set; }

    [BindProperty]
    public string? TargetUserId { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool IsSecureMode { get; set; } = false;

    public UserSession? CurrentSession { get; set; }
    public List<User> AllUsers { get; set; } = new();
    public AdminOperation? OperationResult { get; set; }
    public string? LoginError { get; set; }

    public BrokenAccessControlModel(
        VulnerableAccessControlService vulnerableService,
        SecureAccessControlService secureService,
        ILogger<BrokenAccessControlModel> logger)
    {
        _vulnerableService = vulnerableService;
        _secureService = secureService;
        _logger = logger;
    }

    public void OnGet()
    {
        LoadSession();
        if (CurrentSession?.IsAuthenticated == true)
        {
            LoadUsers();
        }
    }

    public IActionResult OnPostLogin()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            LoginError = "Email e senha são obrigatórios.";
            return Page();
        }

        IAccessControlService service = IsSecureMode ? _secureService : _vulnerableService;
        var session = service.Login(Email, Password);

        if (session != null)
        {
            SaveSession(session);
            return RedirectToPage(new { IsSecureMode });
        }

        LoginError = "Email ou senha inválidos.";
        return Page();
    }

    public IActionResult OnPostLogout()
    {
        ClearSession();
        return RedirectToPage(new { IsSecureMode });
    }

    public IActionResult OnPostDeleteUser()
    {
        LoadSession();
        if (CurrentSession == null || !CurrentSession.IsAuthenticated)
        {
            return RedirectToPage(new { IsSecureMode });
        }

        IAccessControlService service = IsSecureMode ? _secureService : _vulnerableService;
        OperationResult = service.DeleteUser(TargetUserId ?? "", CurrentSession);

        LoadUsers();
        return Page();
    }

    public IActionResult OnPostToggleStatus()
    {
        LoadSession();
        if (CurrentSession == null || !CurrentSession.IsAuthenticated)
        {
            return RedirectToPage(new { IsSecureMode });
        }

        IAccessControlService service = IsSecureMode ? _secureService : _vulnerableService;
        OperationResult = service.ToggleUserStatus(TargetUserId ?? "", CurrentSession);

        LoadUsers();
        return Page();
    }

    public IActionResult OnPostViewSensitiveData()
    {
        LoadSession();
        if (CurrentSession == null || !CurrentSession.IsAuthenticated)
        {
            return RedirectToPage(new { IsSecureMode });
        }

        IAccessControlService service = IsSecureMode ? _secureService : _vulnerableService;
        OperationResult = service.ViewSensitiveData(TargetUserId ?? "", CurrentSession);

        LoadUsers();
        return Page();
    }

    private void LoadSession()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (!string.IsNullOrEmpty(userId))
        {
            CurrentSession = new UserSession
            {
                UserId = userId,
                Name = HttpContext.Session.GetString("Name") ?? "",
                Email = HttpContext.Session.GetString("Email") ?? "",
                Role = HttpContext.Session.GetString("Role") ?? "User",
                IsAuthenticated = true
            };
        }
    }

    private void SaveSession(UserSession session)
    {
        HttpContext.Session.SetString("UserId", session.UserId);
        HttpContext.Session.SetString("Name", session.Name);
        HttpContext.Session.SetString("Email", session.Email);
        HttpContext.Session.SetString("Role", session.Role);
    }

    private void ClearSession()
    {
        HttpContext.Session.Clear();
    }

    private void LoadUsers()
    {
        IAccessControlService service = IsSecureMode ? _secureService : _vulnerableService;
        AllUsers = service.GetAllUsers();
    }
}
