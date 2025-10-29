using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VulnerableWebApp.Models;
using VulnerableWebApp.Services.Database;

namespace VulnerableWebApp.Pages.A03_Injection;

public class SqlInjectionModel : PageModel
{
    private readonly VulnerableDatabaseService _vulnerableService;
    private readonly SecureDatabaseService _secureService;
    private readonly ILogger<SqlInjectionModel> _logger;

    [BindProperty(SupportsGet = true)]
    public string? SearchString { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool IsSecureMode { get; set; } = false;

    public IList<Product> Products { get; set; } = new List<Product>();

    public string? ExecutedQuery { get; set; }

    public string? ErrorMessage { get; set; }

    public SqlInjectionModel(
        VulnerableDatabaseService vulnerableService,
        SecureDatabaseService secureService,
        ILogger<SqlInjectionModel> logger)
    {
        _vulnerableService = vulnerableService;
        _secureService = secureService;
        _logger = logger;
    }

    public void OnGet()
    {
        if (!string.IsNullOrEmpty(SearchString))
        {
            try
            {
                // Seleciona o serviço baseado no modo
                IDatabaseService service = IsSecureMode ? _secureService : _vulnerableService;

                // Gera a query que seria executada (para fins educacionais)
                if (IsSecureMode)
                {
                    ExecutedQuery = $@"SELECT ""Id"", ""Name"", ""Price"" FROM ""Product"" WHERE ""Name"" LIKE @searchString
-- Parâmetro: @searchString = '%{SearchString}%'";
                }
                else
                {
                    ExecutedQuery = $@"SELECT ""Id"", ""Name"", ""Price"" FROM ""Product"" WHERE ""Name"" LIKE '%{SearchString}%'";
                }

                Products = service.SearchProducts(SearchString);

                _logger.LogInformation("Busca realizada ({Mode}): {SearchString} | Resultados: {Count}",
                    IsSecureMode ? "SECURE" : "VULNERABLE", SearchString, Products.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar busca: {Message}", ex.Message);

                // Mostra o erro para fins educacionais
                ErrorMessage = ex.Message;
            }
        }
    }
}
