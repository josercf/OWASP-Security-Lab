using VulnerableWebApp.Models;

namespace VulnerableWebApp.Services.Database;

public interface IDatabaseService
{
    IList<Product> SearchProducts(string searchString);
}
