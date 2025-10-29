using VulnerableWebApp.Models.Data;
using VulnerableWebApp.Services.Database;

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

// Registra o serviço vulnerável (para demonstração)
builder.Services.AddSingleton<IDatabaseService, VulnerableDatabaseService>();

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

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
