using Npgsql;
using System.Data;
using VulnerableWebApp.Models;
using VulnerableWebApp.Models.Data;

namespace VulnerableWebApp.Services.Database;

/// <summary>
/// ATENÇÃO: Este serviço contém INTENCIONALMENTE uma vulnerabilidade de SQL Injection
/// para fins educacionais. NUNCA use este código em produção!
/// </summary>
public class VulnerableDatabaseService : IDatabaseService
{
    private readonly DatabaseConfig _config;
    private readonly ILogger<VulnerableDatabaseService> _logger;

    public VulnerableDatabaseService(DatabaseConfig config, ILogger<VulnerableDatabaseService> logger)
    {
        _config = config;
        _logger = logger;
    }

    private string GetConnectionString()
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = _config.Host,
            Port = _config.Port,
            Database = _config.Database,
            Username = _config.Username,
            Password = _config.Password,
            SslMode = SslMode.Disable,
            TrustServerCertificate = true,
            Pooling = true
        };

        return builder.ToString();
    }

    public IList<Product> SearchProducts(string searchString)
    {
        var products = new List<Product>();

        using (var conn = new NpgsqlConnection(GetConnectionString()))
        {
            using (var cmd = conn.CreateCommand())
            {
                // ⚠️ VULNERABILIDADE INTENCIONAL: SQL Injection
                // O código abaixo concatena diretamente a entrada do usuário na query SQL
                // sem sanitização ou parametrização, permitindo ataques de SQL Injection.
                cmd.CommandText = $@"SELECT ""Id"", ""Name"", ""Price"" FROM ""Product"" WHERE ""Name"" LIKE '%{searchString}%'";

                // Log da query para fins educacionais
                _logger.LogWarning("🔓 QUERY VULNERÁVEL EXECUTADA: {Query}", cmd.CommandText);

                conn.Open();

                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = reader.GetString(0).Trim(),
                            Name = reader.GetString(1).Trim(),
                            Price = reader.GetString(2).Trim()
                        });
                    }
                }

                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
        }

        return products;
    }
}
