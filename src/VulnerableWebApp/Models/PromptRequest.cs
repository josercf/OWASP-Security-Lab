namespace VulnerableWebApp.Models;

public class PromptRequest
{
    public string UserInput { get; set; } = string.Empty;
    public bool UseSecureMode { get; set; } = false;
    public string ApiKey { get; set; } = string.Empty;
}
