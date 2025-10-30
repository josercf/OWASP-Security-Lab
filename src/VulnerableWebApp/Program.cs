using VulnerableWebApp.Models.Data;
using VulnerableWebApp.Services.Database;
using VulnerableWebApp.Services.OpenAI;
using VulnerableWebApp.Services.AccessControl;
using VulnerableWebApp.Services.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configuração do banco de dados
var dbConfig = new DatabaseConfig
{
    Host = builder.Configuration["Database:Host"] ?? "localhost",
    Port = int.Parse(builder.Configuration["Database:Port"] ?? "5432"),
    Database = builder.Configuration["Database:Name"] ?? "vulnerabledb",
    Username = builder.Configuration["Database:Username"] ?? "vulnapp",
    Password = builder.Configuration["Database:Password"] ?? "password123"
};

builder.Services.AddSingleton(dbConfig);

// Configuração do OpenAI
var openAIConfig = new OpenAIConfig
{
    ApiKey = builder.Configuration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty,
    Model = builder.Configuration["OpenAI:Model"] ?? "gpt-4o-mini"
};

builder.Services.AddSingleton(openAIConfig);

// Registra ambos os serviços de database (para comparação educacional)
builder.Services.AddSingleton<VulnerableDatabaseService>();
builder.Services.AddSingleton<SecureDatabaseService>();

// Registra o serviço vulnerável como padrão para IDatabaseService (compatibilidade)
builder.Services.AddSingleton<IDatabaseService, VulnerableDatabaseService>();

// Registra ambos os serviços de OpenAI (para comparação educacional)
builder.Services.AddSingleton<VulnerableOpenAIService>();
builder.Services.AddSingleton<SecureOpenAIService>();

// Registra o serviço vulnerável como padrão para IOpenAIService (compatibilidade)
builder.Services.AddSingleton<IOpenAIService, VulnerableOpenAIService>();

// Registra ambos os serviços de AccessControl (para comparação educacional)
builder.Services.AddSingleton<VulnerableAccessControlService>();
builder.Services.AddSingleton<SecureAccessControlService>();

// Registra o serviço vulnerável como padrão para IAccessControlService (compatibilidade)
builder.Services.AddSingleton<IAccessControlService, VulnerableAccessControlService>();

// Registra ambos os serviços de Logging (para comparação educacional)
builder.Services.AddSingleton<VulnerableLoggingService>();
builder.Services.AddSingleton<SecureLoggingService>();

// Registra o serviço vulnerável como padrão para ISecurityLoggingService (compatibilidade)
builder.Services.AddSingleton<ISecurityLoggingService, VulnerableLoggingService>();

// Configuração de Sessão (necessária para o lab de Broken Access Control)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
