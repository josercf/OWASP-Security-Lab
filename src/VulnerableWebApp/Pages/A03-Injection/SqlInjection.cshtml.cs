using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VulnerableWebApp.Models;
using VulnerableWebApp.Services.Database;

namespace VulnerableWebApp.Pages.A03_Injection;

public class SqlInjectionModel : PageModel
{
    private readonly IDatabaseService _databaseService;
    private readonly ILogger<SqlInjectionModel> _logger;

    [BindProperty(SupportsGet = true)]
    public string? SearchString { get; set; }

    public IList<Product> Products { get; set; } = new List<Product>();

    public string? ExecutedQuery { get; set; }

    public SqlInjectionModel(IDatabaseService databaseService, ILogger<SqlInjectionModel> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    public void OnGet()
    {
        if (!string.IsNullOrEmpty(SearchString))
        {
            try
            {
                // Gera a query que seria executada (para fins educacionais)
                ExecutedQuery = $@"SELECT ""Id"", ""Name"", ""Price"" FROM ""Product"" WHERE ""Name"" LIKE '%{SearchString}%'";

                Products = _databaseService.SearchProducts(SearchString);

                _logger.LogInformation("Busca realizada: {SearchString} | Resultados: {Count}",
                    SearchString, Products.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar busca: {Message}", ex.Message);

                // Mostra o erro para fins educacionais
                TempData["Error"] = $"❌ Erro: {ex.Message}";
            }
        }
    }
}
