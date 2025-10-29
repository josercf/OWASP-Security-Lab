namespace VulnerableWebApp.Models.Data;

public class DatabaseConfig
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string Database { get; set; } = "vulnerabledb";
    public string Username { get; set; } = "vulnapp";
    public string Password { get; set; } = "password123";
}
