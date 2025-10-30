using Npgsql;
using VulnerableWebApp.Models;
using VulnerableWebApp.Models.Data;
using System.Text.Json;

namespace VulnerableWebApp.Services.Logging;

/// <summary>
/// ✅ IMPLEMENTAÇÃO SEGURA
/// Esta implementação demonstra boas práticas de logging de segurança.
/// </summary>
public class SecureLoggingService : ISecurityLoggingService
{
    private readonly DatabaseConfig _config;
    private readonly ILogger<SecureLoggingService> _logger;

    public SecureLoggingService(DatabaseConfig config, ILogger<SecureLoggingService> logger)
    {
        _config = config;
        _logger = logger;
    }

    private string GetConnectionString()
    {
        return $"Host={_config.Host};Port={_config.Port};Database={_config.Database};Username={_config.Username};Password={_config.Password}";
    }

    private void SaveAuditLog(AuditLog log)
    {
        using var conn = new NpgsqlConnection(GetConnectionString());
        conn.Open();

        using var cmd = new NpgsqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"INSERT INTO ""AuditLog""
                           (""Id"", ""Timestamp"", ""Level"", ""EventType"", ""Username"",
                            ""IpAddress"", ""Action"", ""Resource"", ""Success"", ""Message"", ""Details"")
                           VALUES (@id, @timestamp, @level, @eventType, @username,
                                   @ipAddress, @action, @resource, @success, @message, @details)";

        cmd.Parameters.AddWithValue("@id", log.Id);
        cmd.Parameters.AddWithValue("@timestamp", log.Timestamp);
        cmd.Parameters.AddWithValue("@level", log.Level);
        cmd.Parameters.AddWithValue("@eventType", log.EventType);
        cmd.Parameters.AddWithValue("@username", (object?)log.Username ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ipAddress", (object?)log.IpAddress ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@action", (object?)log.Action ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@resource", (object?)log.Resource ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@success", log.Success);
        cmd.Parameters.AddWithValue("@message", (object?)log.Message ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@details", (object?)log.Details ?? DBNull.Value);

        cmd.ExecuteNonQuery();
    }

    public LoggingResult SimulateLoginAttempt(string username, string password, string ipAddress, bool success)
    {
        // ✅ Log completo e estruturado
        var log = new AuditLog
        {
            Timestamp = DateTime.UtcNow,
            Level = success ? "Info" : "Warning",
            EventType = "Login",
            Username = username,
            IpAddress = ipAddress,
            Action = success ? "Login successful" : "Login failed",
            Resource = "Authentication",
            Success = success,
            Message = success
                ? $"User '{username}' successfully authenticated from {ipAddress}"
                : $"Failed login attempt for user '{username}' from {ipAddress}",
            Details = JsonSerializer.Serialize(new
            {
                Username = username,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow.ToString("o"),
                UserAgent = "Mozilla/5.0...", // Seria capturado do request
                Success = success,
                Reason = success ? "Valid credentials" : "Invalid credentials"
            })
        };

        SaveAuditLog(log);

        _logger.LogWarning(
            "🔐 {EventType} | User: {Username} | IP: {IpAddress} | Success: {Success}",
            log.EventType, username, ipAddress, success
        );

        var secureLogDisplay = $@"✅ LOG ESTRUTURADO:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Level: {log.Level}
Event: {log.EventType}
User: {username}
IP: {ipAddress}
Time: {log.Timestamp:yyyy-MM-dd HH:mm:ss} UTC
Success: {success}
Message: {log.Message}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ Salvo no banco de dados para auditoria
✅ Alertas configurados para múltiplas falhas";

        return new LoggingResult
        {
            Success = true,
            Message = success ? "Login registrado com sucesso" : "Falha de login registrada",
            SecureLog = secureLogDisplay,
            MissingInformation = new List<string>(),
            SecurityConcerns = new List<string>()
        };
    }

    public LoggingResult SimulateDataAccess(string username, string resource, string ipAddress)
    {
        // ✅ Log detalhado de acesso a dados
        var log = new AuditLog
        {
            Timestamp = DateTime.UtcNow,
            Level = "Info",
            EventType = "DataAccess",
            Username = username,
            IpAddress = ipAddress,
            Action = "Data read",
            Resource = resource,
            Success = true,
            Message = $"User '{username}' accessed resource '{resource}' from {ipAddress}",
            Details = JsonSerializer.Serialize(new
            {
                Username = username,
                Resource = resource,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow.ToString("o"),
                AccessType = "Read",
                DataClassification = "Sensitive"
            })
        };

        SaveAuditLog(log);

        _logger.LogInformation(
            "📊 {EventType} | User: {Username} | Resource: {Resource} | IP: {IpAddress}",
            log.EventType, username, resource, ipAddress
        );

        var secureLogDisplay = $@"✅ LOG DETALHADO DE ACESSO:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Event: Data Access
User: {username}
Resource: {resource}
IP: {ipAddress}
Time: {log.Timestamp:yyyy-MM-dd HH:mm:ss} UTC
Classification: Sensitive
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ Trilha completa de auditoria
✅ GDPR/LGPD compliance";

        return new LoggingResult
        {
            Success = true,
            Message = "Acesso a dados registrado",
            SecureLog = secureLogDisplay
        };
    }

    public LoggingResult SimulateUnauthorizedAccess(string username, string resource, string ipAddress)
    {
        // ✅ CRÍTICO: Tentativas não autorizadas são registradas com alta prioridade
        var log = new AuditLog
        {
            Timestamp = DateTime.UtcNow,
            Level = "Critical",
            EventType = "UnauthorizedAccess",
            Username = username,
            IpAddress = ipAddress,
            Action = "Unauthorized access attempt",
            Resource = resource,
            Success = false,
            Message = $"⚠️ SECURITY ALERT: User '{username}' attempted unauthorized access to '{resource}' from {ipAddress}",
            Details = JsonSerializer.Serialize(new
            {
                Username = username,
                Resource = resource,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow.ToString("o"),
                Severity = "High",
                RequiresInvestigation = true,
                AlertSent = true
            })
        };

        SaveAuditLog(log);

        _logger.LogCritical(
            "🚨 SECURITY ALERT | {EventType} | User: {Username} | Resource: {Resource} | IP: {IpAddress}",
            log.EventType, username, resource, ipAddress
        );

        var secureLogDisplay = $@"✅ ALERTA DE SEGURANÇA:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🚨 NÍVEL: CRÍTICO
Event: Unauthorized Access Attempt
User: {username}
Resource: {resource}
IP: {ipAddress}
Time: {log.Timestamp:yyyy-MM-dd HH:mm:ss} UTC
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ Equipe de segurança notificada
✅ Correlação com outros eventos ativada
✅ IP pode ser bloqueado após múltiplas tentativas";

        return new LoggingResult
        {
            Success = true,
            Message = "Tentativa de acesso não autorizado REGISTRADA e ALERTADA",
            SecureLog = secureLogDisplay
        };
    }

    public LoggingResult SimulateDataModification(string username, string resource, string oldValue, string newValue, string ipAddress)
    {
        // ✅ Log completo com valores antes/depois
        var log = new AuditLog
        {
            Timestamp = DateTime.UtcNow,
            Level = "Warning",
            EventType = "DataModification",
            Username = username,
            IpAddress = ipAddress,
            Action = "Data modified",
            Resource = resource,
            Success = true,
            Message = $"User '{username}' modified '{resource}' from {ipAddress}",
            Details = JsonSerializer.Serialize(new
            {
                Username = username,
                Resource = resource,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow.ToString("o"),
                OldValue = oldValue,
                NewValue = newValue,
                ChangeType = "Update"
            })
        };

        SaveAuditLog(log);

        _logger.LogWarning(
            "✏️ {EventType} | User: {Username} | Resource: {Resource} | IP: {IpAddress}",
            log.EventType, username, resource, ipAddress
        );

        var secureLogDisplay = $@"✅ LOG DE MODIFICAÇÃO:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Event: Data Modification
User: {username}
Resource: {resource}
IP: {ipAddress}
Time: {log.Timestamp:yyyy-MM-dd HH:mm:ss} UTC
Old Value: {oldValue}
New Value: {newValue}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ Reversão possível com histórico completo
✅ Compliance atendido (auditoria)";

        return new LoggingResult
        {
            Success = true,
            Message = "Modificação registrada com histórico completo",
            SecureLog = secureLogDisplay
        };
    }

    public LoggingResult SimulateSuspiciousActivity(string username, string activity, string ipAddress)
    {
        // ✅ Atividade suspeita registrada com detalhes
        var log = new AuditLog
        {
            Timestamp = DateTime.UtcNow,
            Level = "Critical",
            EventType = "SuspiciousActivity",
            Username = username,
            IpAddress = ipAddress,
            Action = "Suspicious pattern detected",
            Resource = "System",
            Success = false,
            Message = $"⚠️ SUSPICIOUS ACTIVITY: {activity} by user '{username}' from {ipAddress}",
            Details = JsonSerializer.Serialize(new
            {
                Username = username,
                Activity = activity,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow.ToString("o"),
                Pattern = "Anomaly detected",
                RiskScore = 8.5,
                AutoResponse = "Monitoring increased"
            })
        };

        SaveAuditLog(log);

        _logger.LogCritical(
            "🚨 SUSPICIOUS ACTIVITY | {EventType} | User: {Username} | Activity: {Activity} | IP: {IpAddress}",
            log.EventType, username, activity, ipAddress
        );

        var secureLogDisplay = $@"✅ DETECÇÃO DE ANOMALIA:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🚨 NÍVEL: CRÍTICO
Event: Suspicious Activity
User: {username}
Activity: {activity}
IP: {ipAddress}
Time: {log.Timestamp:yyyy-MM-dd HH:mm:ss} UTC
Risk Score: 8.5/10
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ SOC (Security Operations Center) alertado
✅ Correlação automática com outros eventos
✅ Resposta automatizada ativada";

        return new LoggingResult
        {
            Success = true,
            Message = "Atividade suspeita detectada e registrada",
            SecureLog = secureLogDisplay
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
