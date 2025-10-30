namespace VulnerableWebApp.Models;

public class SecurityEvent
{
    public string EventType { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public enum EventType
{
    Login,
    Logout,
    DataAccess,
    DataModification,
    DataDeletion,
    UnauthorizedAccess,
    ConfigurationChange,
    PasswordChange,
    PrivilegeEscalation,
    SuspiciousActivity
}
