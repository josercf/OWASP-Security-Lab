using Npgsql;
using VulnerableWebApp.Models;
using VulnerableWebApp.Models.Data;

namespace VulnerableWebApp.Services.Logging;

/// <summary>
/// ⚠️ IMPLEMENTAÇÃO VULNERÁVEL - APENAS PARA FINS EDUCACIONAIS
/// Esta implementação demonstra FALHAS GRAVES em logging de segurança.
/// NÃO USE EM PRODUÇÃO!
/// </summary>
public class VulnerableLoggingService : ISecurityLoggingService
{
    private readonly DatabaseConfig _config;
    private readonly ILogger<VulnerableLoggingService> _logger;

    public VulnerableLoggingService(DatabaseConfig config, ILogger<VulnerableLoggingService> logger)
    {
        _config = config;
        _logger = logger;
    }

    private string GetConnectionString()
    {
        return $"Host={_config.Host};Port={_config.Port};Database={_config.Database};Username={_config.Username};Password={_config.Password}";
    }

    public LoggingResult SimulateLoginAttempt(string username, string password, string ipAddress, bool success)
    {
        // ⚠️ VULNERABILIDADE 1: Log genérico e vago
        // ⚠️ VULNERABILIDADE 2: Não registra IP, timestamp preciso, ou detalhes
        // ⚠️ VULNERABILIDADE 3: Log só de sucessos, ignora falhas

        if (success)
        {
            _logger.LogInformation("User logged in");
            // Não salva no banco - apenas console log!
        }
        // ⚠️ Falhas de login NÃO são registradas!

        var result = new LoggingResult
        {
            Success = true,
            Message = success ? "Login realizado" : "Login falhou",
            VulnerableLog = success ? "INFO: User logged in" : "(Sem log)",
            MissingInformation = new List<string>
            {
                "❌ Username não registrado",
                "❌ IP address não registrado",
                "❌ Timestamp preciso não registrado",
                success ? "" : "❌ Falhas de login NÃO são registradas!",
                "❌ Não persistido no banco de dados"
            },
            SecurityConcerns = new List<string>
            {
                "🚨 Impossível detectar ataques de força bruta",
                "🚨 Não há auditoria de acessos",
                "🚨 Impossível investigar incidentes"
            }
        };

        return result;
    }

    public LoggingResult SimulateDataAccess(string username, string resource, string ipAddress)
    {
        // ⚠️ VULNERABILIDADE: Nenhum log de acesso a dados sensíveis

        _logger.LogDebug("Data accessed"); // Debug level - geralmente desabilitado em produção!

        return new LoggingResult
        {
            Success = true,
            Message = "Dados acessados",
            VulnerableLog = "DEBUG: Data accessed",
            MissingInformation = new List<string>
            {
                "❌ Quem acessou? (usuário não registrado)",
                "❌ O que foi acessado? (recurso não registrado)",
                "❌ Quando? (timestamp não registrado)",
                "❌ De onde? (IP não registrado)",
                "❌ Log em nível DEBUG (desabilitado em produção)"
            },
            SecurityConcerns = new List<string>
            {
                "🚨 Exfiltração de dados não detectável",
                "🚨 Acesso não autorizado passa despercebido",
                "🚨 Sem trilha de auditoria"
            }
        };
    }

    public LoggingResult SimulateUnauthorizedAccess(string username, string resource, string ipAddress)
    {
        // ⚠️ VULNERABILIDADE CRÍTICA: Tentativas de acesso não autorizado NÃO são registradas!

        // Silêncio total - nenhum log!

        return new LoggingResult
        {
            Success = false,
            Message = "Acesso não autorizado tentado",
            VulnerableLog = "(SEM LOG - Silêncio total!)",
            MissingInformation = new List<string>
            {
                "❌ NENHUM registro da tentativa!",
                "❌ Atacante pode tentar infinitamente sem detecção",
                "❌ Zero visibilidade para equipe de segurança"
            },
            SecurityConcerns = new List<string>
            {
                "🚨 CRÍTICO: Ataques não são detectados",
                "🚨 CRÍTICO: Sem alertas de segurança",
                "🚨 CRÍTICO: Compliance não atendido"
            }
        };
    }

    public LoggingResult SimulateDataModification(string username, string resource, string oldValue, string newValue, string ipAddress)
    {
        // ⚠️ VULNERABILIDADE: Log muito vago, sem detalhes

        _logger.LogInformation("Data changed");

        return new LoggingResult
        {
            Success = true,
            Message = "Dados modificados",
            VulnerableLog = "INFO: Data changed",
            MissingInformation = new List<string>
            {
                "❌ Quem modificou?",
                "❌ O que foi modificado?",
                "❌ Valor anterior não registrado",
                "❌ Valor novo não registrado",
                "❌ Sem contexto para auditoria"
            },
            SecurityConcerns = new List<string>
            {
                "🚨 Impossível reverter mudanças maliciosas",
                "🚨 Sem trilha de auditoria adequada",
                "🚨 Compliance não atendido (GDPR, LGPD)"
            }
        };
    }

    public LoggingResult SimulateSuspiciousActivity(string username, string activity, string ipAddress)
    {
        // ⚠️ VULNERABILIDADE: Atividade suspeita não gera alerta

        _logger.LogWarning("Something happened");

        return new LoggingResult
        {
            Success = true,
            Message = "Atividade suspeita detectada",
            VulnerableLog = "WARN: Something happened",
            MissingInformation = new List<string>
            {
                "❌ Mensagem genérica e inútil",
                "❌ Sem detalhes da atividade suspeita",
                "❌ Sem contexto do usuário ou IP",
                "❌ Não gera alerta para equipe de segurança"
            },
            SecurityConcerns = new List<string>
            {
                "🚨 Ataques em andamento não são detectados",
                "🚨 Sem resposta a incidentes",
                "🚨 Tempo de detecção: INFINITO"
            }
        };
    }

    public List<AuditLog> GetRecentLogs(int count = 50)
    {
        var logs = new List<AuditLog>();

        using var conn = new NpgsqlConnection(GetConnectionString());
        conn.Open();

        using var cmd = new NpgsqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"SELECT ""Id"", ""Timestamp"", ""Level"", ""EventType"", ""Username"",
                                  ""IpAddress"", ""Action"", ""Resource"", ""Success"", ""Message"", ""Details""
                           FROM ""AuditLog""
                           ORDER BY ""Timestamp"" DESC
                           LIMIT @count";
        cmd.Parameters.AddWithValue("@count", count);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            logs.Add(new AuditLog
            {
                Id = reader.GetString(0),
                Timestamp = reader.GetDateTime(1),
                Level = reader.GetString(2),
                EventType = reader.GetString(3),
                Username = reader.IsDBNull(4) ? "" : reader.GetString(4),
                IpAddress = reader.IsDBNull(5) ? "" : reader.GetString(5),
                Action = reader.IsDBNull(6) ? "" : reader.GetString(6),
                Resource = reader.IsDBNull(7) ? "" : reader.GetString(7),
                Success = reader.GetBoolean(8),
                Message = reader.IsDBNull(9) ? "" : reader.GetString(9),
                Details = reader.IsDBNull(10) ? "" : reader.GetString(10)
            });
        }

        return logs;
    }

    public void ClearLogs()
    {
        using var conn = new NpgsqlConnection(GetConnectionString());
        conn.Open();

        using var cmd = new NpgsqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"DELETE FROM ""AuditLog""";
        cmd.ExecuteNonQuery();
    }
}
