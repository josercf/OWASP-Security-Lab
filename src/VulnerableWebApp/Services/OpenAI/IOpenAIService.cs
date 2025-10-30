using VulnerableWebApp.Models;

namespace VulnerableWebApp.Services.OpenAI;

public interface IOpenAIService
{
    Task<PromptResponse> ProcessPromptAsync(PromptRequest request);
    bool IsConfigured();
}
