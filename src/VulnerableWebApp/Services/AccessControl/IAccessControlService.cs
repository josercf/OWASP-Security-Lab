using VulnerableWebApp.Models;

namespace VulnerableWebApp.Services.AccessControl;

public interface IAccessControlService
{
    UserSession? Login(string email, string password);
    List<User> GetAllUsers();
    AdminOperation DeleteUser(string userId, UserSession currentUser);
    AdminOperation ToggleUserStatus(string userId, UserSession currentUser);
    AdminOperation ViewSensitiveData(string userId, UserSession currentUser);
}
