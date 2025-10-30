namespace VulnerableWebApp.Models;

public class LoggingResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string VulnerableLog { get; set; } = string.Empty;
    public string SecureLog { get; set; } = string.Empty;
    public List<string> MissingInformation { get; set; } = new();
    public List<string> SecurityConcerns { get; set; } = new();
}
