namespace VulnerableWebApp.Models;

public class PromptResponse
{
    public string Response { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public string FinalPrompt { get; set; } = string.Empty;
    public bool IsSecure { get; set; }
    public string Error { get; set; } = string.Empty;
}
