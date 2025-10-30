namespace VulnerableWebApp.Models;

public class UserSession
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public bool IsAuthenticated { get; set; } = false;
    public bool IsAdmin => Role == "Admin";
}
