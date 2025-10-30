using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VulnerableWebApp.Models;
using VulnerableWebApp.Services.OpenAI;

namespace VulnerableWebApp.Pages.A03_Injection;

public class PromptInjectionModel : PageModel
{
    private readonly VulnerableOpenAIService _vulnerableService;
    private readonly SecureOpenAIService _secureService;
    private readonly ILogger<PromptInjectionModel> _logger;

    [BindProperty]
    public string? UserInput { get; set; }

    [BindProperty]
    public string? ApiKey { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool IsSecureMode { get; set; } = false;

    public new PromptResponse? Response { get; set; }

    public PromptInjectionModel(
        VulnerableOpenAIService vulnerableService,
        SecureOpenAIService secureService,
        ILogger<PromptInjectionModel> logger)
    {
        _vulnerableService = vulnerableService;
        _secureService = secureService;
        _logger = logger;
    }

    public void OnGet()
    {
        // Nada a fazer no Get, usuário fornecerá a API key
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            Response = new PromptResponse
            {
                Error = "Por favor, forneça sua OpenAI API Key."
            };
            return Page();
        }

        if (string.IsNullOrWhiteSpace(UserInput))
        {
            Response = new PromptResponse
            {
                Error = "Por favor, digite um comentário para análise."
            };
            return Page();
        }

        try
        {
            var request = new PromptRequest
            {
                UserInput = UserInput,
                UseSecureMode = IsSecureMode,
                ApiKey = ApiKey
            };

            // Seleciona o serviço baseado no modo
            IOpenAIService service = IsSecureMode ? _secureService : _vulnerableService;

            Response = await service.ProcessPromptAsync(request);

            _logger.LogInformation("Análise realizada ({Mode}): {Input} | Sucesso: {Success}",
                IsSecureMode ? "SECURE" : "VULNERABLE", UserInput, string.IsNullOrEmpty(Response.Error));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar prompt: {Message}", ex.Message);

            Response = new PromptResponse
            {
                Error = $"Erro ao processar: {ex.Message}"
            };
        }

        return Page();
    }
}
