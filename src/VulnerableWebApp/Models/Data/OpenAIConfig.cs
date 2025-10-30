namespace VulnerableWebApp.Models.Data;

public class OpenAIConfig
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}
