using Npgsql;
using System.Data;
using VulnerableWebApp.Models;
using VulnerableWebApp.Models.Data;

namespace VulnerableWebApp.Services.Database;

/// <summary>
/// Implementação SEGURA usando consultas parametrizadas.
/// Esta versão protege contra SQL Injection.
/// </summary>
public class SecureDatabaseService : IDatabaseService
{
    private readonly DatabaseConfig _config;
    private readonly ILogger<SecureDatabaseService> _logger;

    public SecureDatabaseService(DatabaseConfig config, ILogger<SecureDatabaseService> logger)
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
                // ✅ CÓDIGO SEGURO: Usa consulta parametrizada
                // O parâmetro @searchString é tratado como dado, não como código SQL
                cmd.CommandText = @"SELECT ""Id"", ""Name"", ""Price"" FROM ""Product"" WHERE ""Name"" LIKE @searchString";
                cmd.Parameters.AddWithValue("@searchString", $"%{searchString}%");

                // Log da query para fins educacionais
                _logger.LogInformation("🔒 QUERY SEGURA EXECUTADA: {Query} | Parâmetro: {Param}",
                    cmd.CommandText, searchString);

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
