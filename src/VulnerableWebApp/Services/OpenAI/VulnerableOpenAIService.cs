using VulnerableWebApp.Models;
using OpenAI.Chat;

namespace VulnerableWebApp.Services.OpenAI;

/// <summary>
/// ⚠️ IMPLEMENTAÇÃO VULNERÁVEL - APENAS PARA FINS EDUCACIONAIS
/// Esta implementação é INTENCIONALMENTE vulnerável a Prompt Injection.
/// NÃO USE EM PRODUÇÃO!
/// </summary>
public class VulnerableOpenAIService : IOpenAIService
{
    private readonly ILogger<VulnerableOpenAIService> _logger;
    private const string DefaultModel = "gpt-4o-mini";

    public VulnerableOpenAIService(ILogger<VulnerableOpenAIService> logger)
    {
        _logger = logger;
    }

    public bool IsConfigured()
    {
        // Sempre retorna false pois agora usa API key do request
        return false;
    }

    public async Task<PromptResponse> ProcessPromptAsync(PromptRequest request)
    {
        var response = new PromptResponse
        {
            IsSecure = false
        };

        if (string.IsNullOrWhiteSpace(request.ApiKey))
        {
            response.Error = "OpenAI API Key não fornecida. Por favor, insira sua chave no campo acima.";
            return response;
        }

        try
        {
            // ⚠️ VULNERABILIDADE: Concatenação direta do input do usuário no prompt
            // O input do usuário não é sanitizado e é inserido diretamente no contexto
            var systemPrompt = "Você é um assistente de análise de sentimentos de produtos. " +
                              "Sua única função é analisar comentários sobre produtos e classificá-los como POSITIVO, NEGATIVO ou NEUTRO. " +
                              "Você deve SEMPRE responder apenas com a classificação seguida de uma breve justificativa (máximo 20 palavras).";

            // ⚠️ PROBLEMA: Input do usuário é concatenado diretamente sem validação
            var userPrompt = $"Analise o seguinte comentário sobre o produto:\n\n{request.UserInput}";

            response.SystemPrompt = systemPrompt;
            response.FinalPrompt = userPrompt;

            _logger.LogWarning("🔓 PROMPT VULNERÁVEL - Input não sanitizado: {UserInput}", request.UserInput);

            var client = new ChatClient(DefaultModel, request.ApiKey);

            var chatMessages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            };

            var completion = await client.CompleteChatAsync(chatMessages);

            response.Response = completion.Value.Content[0].Text;

            _logger.LogInformation("✅ Resposta recebida da OpenAI");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao processar prompt");
            response.Error = $"Erro: {ex.Message}";
        }

        return response;
    }
}
