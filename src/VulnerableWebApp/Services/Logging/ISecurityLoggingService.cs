using VulnerableWebApp.Models;

namespace VulnerableWebApp.Services.Logging;

public interface ISecurityLoggingService
{
    LoggingResult SimulateLoginAttempt(string username, string password, string ipAddress, bool success);
    LoggingResult SimulateDataAccess(string username, string resource, string ipAddress);
    LoggingResult SimulateUnauthorizedAccess(string username, string resource, string ipAddress);
    LoggingResult SimulateDataModification(string username, string resource, string oldValue, string newValue, string ipAddress);
    LoggingResult SimulateSuspiciousActivity(string username, string activity, string ipAddress);
    List<AuditLog> GetRecentLogs(int count = 50);
    void ClearLogs();
}
