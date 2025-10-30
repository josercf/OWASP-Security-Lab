using Npgsql;
using VulnerableWebApp.Models;
using VulnerableWebApp.Models.Data;

namespace VulnerableWebApp.Services.AccessControl;

/// <summary>
/// ✅ IMPLEMENTAÇÃO SEGURA
/// Esta implementação demonstra boas práticas de controle de acesso.
/// </summary>
public class SecureAccessControlService : IAccessControlService
{
    private readonly DatabaseConfig _config;
    private readonly ILogger<SecureAccessControlService> _logger;

    public SecureAccessControlService(DatabaseConfig config, ILogger<SecureAccessControlService> logger)
    {
        _config = config;
        _logger = logger;
    }

    private string GetConnectionString()
    {
        return $"Host={_config.Host};Port={_config.Port};Database={_config.Database};Username={_config.Username};Password={_config.Password}";
    }

    public UserSession? Login(string email, string password)
    {
        using var conn = new NpgsqlConnection(GetConnectionString());
        conn.Open();

        using var cmd = new NpgsqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"SELECT ""Id"", ""Name"", ""Email"", ""Role"", ""IsActive""
                           FROM ""Users""
                           WHERE ""Email"" = @email AND ""Password"" = @password AND ""IsActive"" = true";
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@password", password);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var session = new UserSession
            {
                UserId = reader.GetString(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                Role = reader.GetString(3),
                IsAuthenticated = true
            };

            _logger.LogInformation("✅ Login realizado: {Email} - Role: {Role}", session.Email, session.Role);
            return session;
        }

        _logger.LogWarning("❌ Falha no login: {Email}", email);
        return null;
    }

    public List<User> GetAllUsers()
    {
        var users = new List<User>();

        using var conn = new NpgsqlConnection(GetConnectionString());
        conn.Open();

        using var cmd = new NpgsqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"SELECT ""Id"", ""Name"", ""Email"", ""Password"", ""Role"", ""IsActive""
                           FROM ""Users""
                           ORDER BY ""Role"" DESC, ""Name""";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new User
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                Password = reader.GetString(3),
                Role = reader.GetString(4),
                IsActive = reader.GetBoolean(5)
            });
        }

        return users;
    }

    private bool IsAdmin(UserSession user)
    {
        return user.IsAuthenticated && user.Role == "Admin";
    }

    public AdminOperation DeleteUser(string userId, UserSession currentUser)
    {
        // ✅ VALIDAÇÃO: Verifica se o usuário é Admin
        if (!IsAdmin(currentUser))
        {
            _logger.LogWarning("🔒 ACESSO NEGADO - Usuário {Email} (Role: {Role}) tentou deletar usuário {TargetUser}",
                currentUser.Email, currentUser.Role, userId);

            return new AdminOperation
            {
                Operation = "Delete",
                TargetUserId = userId,
                Success = false,
                WasAuthorized = false,
                Message = "❌ ACESSO NEGADO! Apenas administradores podem deletar usuários."
            };
        }

        if (userId == currentUser.UserId)
        {
            return new AdminOperation
            {
                Operation = "Delete",
                TargetUserId = userId,
                Success = false,
                WasAuthorized = false,
                Message = "Você não pode deletar sua própria conta!"
            };
        }

        using var conn = new NpgsqlConnection(GetConnectionString());
        conn.Open();

        using var cmd = new NpgsqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"DELETE FROM ""Users"" WHERE ""Id"" = @userId";
        cmd.Parameters.AddWithValue("@userId", userId);

        var rowsAffected = cmd.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            _logger.LogInformation("✅ Usuário {UserId} deletado pelo admin {CurrentUser}", userId, currentUser.Email);
            return new AdminOperation
            {
                Operation = "Delete",
                TargetUserId = userId,
                Success = true,
                WasAuthorized = true,
                Message = "✅ Usuário deletado com sucesso! Operação autorizada."
            };
        }

        return new AdminOperation
        {
            Operation = "Delete",
            TargetUserId = userId,
            Success = false,
            WasAuthorized = true,
            Message = "Usuário não encontrado."
        };
    }

    public AdminOperation ToggleUserStatus(string userId, UserSession currentUser)
    {
        // ✅ VALIDAÇÃO: Verifica se o usuário é Admin
        if (!IsAdmin(currentUser))
        {
            _logger.LogWarning("🔒 ACESSO NEGADO - Usuário {Email} (Role: {Role}) tentou alterar status de {TargetUser}",
                currentUser.Email, currentUser.Role, userId);

            return new AdminOperation
            {
                Operation = "ToggleStatus",
                TargetUserId = userId,
                Success = false,
                WasAuthorized = false,
                Message = "❌ ACESSO NEGADO! Apenas administradores podem alterar status de usuários."
            };
        }

        if (userId == currentUser.UserId)
        {
            return new AdminOperation
            {
                Operation = "ToggleStatus",
                TargetUserId = userId,
                Success = false,
                WasAuthorized = false,
                Message = "Você não pode alterar o status da sua própria conta!"
            };
        }

        using var conn = new NpgsqlConnection(GetConnectionString());
        conn.Open();

        using var cmd = new NpgsqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"UPDATE ""Users"" SET ""IsActive"" = NOT ""IsActive"" WHERE ""Id"" = @userId RETURNING ""IsActive""";
        cmd.Parameters.AddWithValue("@userId", userId);

        var result = cmd.ExecuteScalar();

        if (result != null)
        {
            var newStatus = (bool)result;
            _logger.LogInformation("✅ Status do usuário {UserId} alterado pelo admin {CurrentUser}", userId, currentUser.Email);
            return new AdminOperation
            {
                Operation = "ToggleStatus",
                TargetUserId = userId,
                Success = true,
                WasAuthorized = true,
                Message = $"✅ Status alterado para: {(newStatus ? "Ativo" : "Inativo")}. Operação autorizada."
            };
        }

        return new AdminOperation
        {
            Operation = "ToggleStatus",
            TargetUserId = userId,
            Success = false,
            WasAuthorized = true,
            Message = "Usuário não encontrado."
        };
    }

    public AdminOperation ViewSensitiveData(string userId, UserSession currentUser)
    {
        // ✅ VALIDAÇÃO: Verifica se o usuário é Admin
        if (!IsAdmin(currentUser))
        {
            _logger.LogWarning("🔒 ACESSO NEGADO - Usuário {Email} (Role: {Role}) tentou acessar dados de {TargetUser}",
                currentUser.Email, currentUser.Role, userId);

            return new AdminOperation
            {
                Operation = "ViewSensitiveData",
                TargetUserId = userId,
                Success = false,
                WasAuthorized = false,
                Message = "❌ ACESSO NEGADO! Apenas administradores podem visualizar dados sensíveis."
            };
        }

        using var conn = new NpgsqlConnection(GetConnectionString());
        conn.Open();

        using var cmd = new NpgsqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"SELECT ""Name"", ""Email"", ""Password"", ""Role""
                           FROM ""Users""
                           WHERE ""Id"" = @userId";
        cmd.Parameters.AddWithValue("@userId", userId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var sensitiveData = $@"
📋 DADOS SENSÍVEIS DO USUÁRIO:
━━━━━━━━━━━━━━━━━━━━━━━━━━
Nome: {reader.GetString(0)}
Email: {reader.GetString(1)}
Senha: {reader.GetString(2)} ⚠️ (texto plano!)
Role: {reader.GetString(3)}
━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ Acesso autorizado - Usuário Admin";

            _logger.LogInformation("✅ Dados sensíveis acessados pelo admin {CurrentUser}", currentUser.Email);

            return new AdminOperation
            {
                Operation = "ViewSensitiveData",
                TargetUserId = userId,
                Success = true,
                WasAuthorized = true,
                Message = sensitiveData
            };
        }

        return new AdminOperation
        {
            Operation = "ViewSensitiveData",
            TargetUserId = userId,
            Success = false,
            WasAuthorized = true,
            Message = "Usuário não encontrado."
        };
    }
}
