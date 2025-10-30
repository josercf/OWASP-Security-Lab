namespace VulnerableWebApp.Models;

public class AdminOperation
{
    public string Operation { get; set; } = string.Empty;
    public string TargetUserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
    public bool WasAuthorized { get; set; }
}
