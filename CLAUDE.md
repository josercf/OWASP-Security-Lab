# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**OWASP Security Lab** is an educational web application demonstrating security vulnerabilities from the OWASP Top 10 - 2021. Built with ASP.NET Core (.NET 10) and PostgreSQL, it provides hands-on learning through intentionally vulnerable code implementations.

**CRITICAL**: This codebase contains intentionally insecure code for educational purposes. Do NOT deploy to production or fix vulnerabilities unless explicitly requested - they are designed for demonstration.

## Technology Stack

- **.NET 10 RC** (net10.0) - Latest LTS preview
- **ASP.NET Core Razor Pages** - Web framework
- **Npgsql 9.0.4** - PostgreSQL driver
- **PostgreSQL 16** - Database
- **Docker & Docker Compose** - Containerization
- **Bootstrap 5** - UI framework

## Development Commands

### Docker Operations (Primary Method)

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f webapp
docker-compose logs -f postgres

# Restart application only
docker-compose restart webapp

# Rebuild after code changes
docker-compose up -d --build webapp

# Stop all services
docker-compose down

# Stop and remove volumes (fresh start)
docker-compose down -v
```

### Local Development (Without Docker)

```bash
# Restore dependencies
cd src/VulnerableWebApp
dotnet restore

# Build
dotnet build

# Run application
dotnet run

# Watch mode (auto-reload)
dotnet watch run
```

### Database Operations

```bash
# Start only database
docker-compose up -d postgres

# Connect to PostgreSQL
docker exec -it owasp-lab-db psql -U postgres -d vulnerabledb

# Reset database (remove and recreate)
docker-compose down -v
docker-compose up -d postgres
```

### Build and Test

```bash
# Build project
cd src/VulnerableWebApp
dotnet build -c Release

# Build Docker image manually
docker build -t owasp-lab:latest .

# Run tests (when implemented)
dotnet test
```

## Architecture

### High-Level Structure

```
┌─────────────────────────────────────────┐
│         Browser (localhost:5000)        │
└─────────────────┬───────────────────────┘
                  │ HTTP
┌─────────────────▼───────────────────────┐
│   ASP.NET Core Razor Pages (.NET 10)    │
│   ┌─────────────────────────────────┐   │
│   │  Pages/                         │   │
│   │  ├── Index.cshtml (Dashboard)   │   │
│   │  └── A03-Injection/             │   │
│   │      └── SqlInjection.cshtml    │   │
│   └─────────────┬───────────────────┘   │
│                 │                        │
│   ┌─────────────▼───────────────────┐   │
│   │  Services/Database/             │   │
│   │  ├── IDatabaseService           │   │
│   │  ├── VulnerableDatabaseService  │   │
│   │  └── SecureDatabaseService      │   │
│   └─────────────┬───────────────────┘   │
└─────────────────┼───────────────────────┘
                  │ Npgsql
┌─────────────────▼───────────────────────┐
│      PostgreSQL 16 (port 5432)          │
│      ├── vulnerabledb                   │
│      │   ├── Product table              │
│      │   └── Users table                │
└─────────────────────────────────────────┘
```

### Key Components

#### 1. Models (`Models/`)

- **Product.cs**: Product entity (Id, Name, Price)
- **Data/DatabaseConfig.cs**: Database connection configuration

#### 2. Services (`Services/Database/`)

- **IDatabaseService**: Interface for database operations
- **VulnerableDatabaseService**: INTENTIONALLY vulnerable implementation using string concatenation for SQL queries (SQL Injection vulnerability)
- **SecureDatabaseService**: Secure implementation using parameterized queries (for comparison)

#### 3. Pages (`Pages/`)

- **Index.cshtml**: Dashboard displaying all OWASP Top 10 categories (cards)
- **A03-Injection/SqlInjection.cshtml**: SQL Injection lab page
  - Search form for products
  - Displays executed SQL query (educational)
  - Shows example payloads
  - Results table

#### 4. Configuration

- **Program.cs**: Application startup and dependency injection
  - Registers DatabaseConfig from appsettings
  - Registers VulnerableDatabaseService as singleton
  - Configures Razor Pages

- **appsettings.json**: Production settings (Docker)
  - Database host: "postgres"

- **appsettings.Development.json**: Local development settings
  - Database host: "localhost"

### Critical Code Patterns

#### Vulnerable SQL Query (INTENTIONAL)

Location: `Services/Database/VulnerableDatabaseService.cs:52`

```csharp
// ⚠️ INTENTIONAL VULNERABILITY
cmd.CommandText = $@"SELECT ""Id"", ""Name"", ""Price"" FROM ""Product"" WHERE ""Name"" LIKE '%{searchString}%'";
```

This uses string interpolation, allowing SQL injection attacks.

#### Secure SQL Query (Reference)

Location: `Services/Database/SecureDatabaseService.cs`

```csharp
// ✅ SECURE - Uses parameterized query
cmd.CommandText = @"SELECT ""Id"", ""Name"", ""Price"" FROM ""Product"" WHERE ""Name"" LIKE @searchString";
cmd.Parameters.AddWithValue("@searchString", $"%{searchString}%");
```

#### Dependency Injection Pattern

Location: `Program.cs:6-19`

```csharp
var dbConfig = new DatabaseConfig
{
    Host = builder.Configuration["Database:Host"] ?? "localhost",
    Port = int.Parse(builder.Configuration["Database:Port"] ?? "5432"),
    Database = builder.Configuration["Database:Name"] ?? "vulnerabledb",
    Username = builder.Configuration["Database:Username"] ?? "vulnapp",
    Password = builder.Configuration["Database:Password"] ?? "password123"
};

builder.Services.AddSingleton(dbConfig);
builder.Services.AddSingleton<IDatabaseService, VulnerableDatabaseService>();
```

## Database Schema

### Product Table

```sql
CREATE TABLE "Product" (
    "Id" VARCHAR(120) NOT NULL PRIMARY KEY,
    "Name" VARCHAR(120) NOT NULL,
    "Price" VARCHAR(120) NOT NULL
);
```

Sample data: 8 products (Hammer, Driver-Drills, Hammer Drills, Rotary Drills, etc.)

### Users Table

```sql
CREATE TABLE "Users" (
    "Id" VARCHAR(120) NOT NULL PRIMARY KEY,
    "Name" VARCHAR(120) NOT NULL,
    "Password" VARCHAR(120) NOT NULL  -- ⚠️ Plain text for educational purposes
);
```

Sample data: 4 users (admin/admin123, john.doe/password123, etc.)

**Note**: PostgreSQL requires quoted identifiers for case-sensitive table/column names: `"Product"`, `"Name"`, etc.

## Adding New Vulnerability Labs

To add a new OWASP vulnerability lab (e.g., A01, A02):

1. **Create directory structure**:
   ```bash
   mkdir -p src/VulnerableWebApp/Pages/A01-BrokenAccessControl
   ```

2. **Create Razor Page**:
   - `A01-BrokenAccessControl/Lab.cshtml` - View
   - `A01-BrokenAccessControl/Lab.cshtml.cs` - Page model

3. **Implement vulnerability**:
   - Create service with intentional vulnerability
   - Create secure version for comparison
   - Add to dependency injection in `Program.cs`

4. **Update dashboard** (`Pages/Index.cshtml`):
   - Change card from `border-secondary` to `border-danger shadow`
   - Change button from `disabled` to active link
   - Add `badge bg-success` with "✓ Disponível"

5. **Create documentation**:
   - `docs/labs/A01-BrokenAccessControl.md`
   - Follow template from `A03-SQL-Injection.md`

6. **Add database migrations if needed**:
   - Create new SQL file in `database/init/`
   - Use sequential numbering (02-*, 03-*, etc.)

## Environment Configuration

### Environment Variables (Docker)

Set in `docker-compose.yml` for webapp service:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Production
  - Database__Host=postgres
  - Database__Port=5432
  - Database__Name=vulnerabledb
  - Database__Username=vulnapp
  - Database__Password=password123
```

### Configuration Hierarchy

1. Environment variables (highest priority)
2. appsettings.{Environment}.json
3. appsettings.json (lowest priority)

### Database Connection

- **Development**: localhost:5432
- **Docker**: postgres:5432 (container name)
- **Credentials**: vulnapp / password123

## Common Tasks

### Adding New Dependencies

```bash
cd src/VulnerableWebApp
dotnet add package PackageName
```

### Updating Bootstrap/CSS

Static files are in `wwwroot/`:
- `wwwroot/css/` - Custom styles
- `wwwroot/js/` - Custom JavaScript
- `wwwroot/lib/` - Third-party libraries

### Logging

Application uses built-in ASP.NET Core logging:

```csharp
_logger.LogWarning("🔓 QUERY VULNERÁVEL: {Query}", cmd.CommandText);
_logger.LogInformation("🔒 QUERY SEGURA: {Query}", cmd.CommandText);
```

View logs: `docker-compose logs -f webapp`

## Testing SQL Injection Lab

### Example Payloads

1. **Basic bypass**: `' OR '1'='1`
2. **Comment out**: `' OR 1=1 --`
3. **UNION attack**: `' UNION SELECT "Id","Name","Password" FROM "Users" --`
4. **Stacked queries**: `'; DROP TABLE "Product"; --`

### Expected Behavior

- Vulnerable version executes concatenated SQL directly
- Query is displayed in yellow alert box
- Results show in table below
- Logs show executed query with 🔓 emoji

## Important Notes

- **PostgreSQL Identifiers**: Always use double quotes for table/column names: `"Product"`, `"Name"`
- **.NET 10**: Uses preview/RC SDK, may have breaking changes
- **Port 8080**: Container exposes 8080, mapped to 5000 on host
- **Kestrel**: Web server runs on port 8080 inside container
- **Hot Reload**: Use `dotnet watch` for development, or rebuild Docker image

## Future Labs Roadmap

Planned vulnerabilities to implement:

- **A01** - Broken Access Control
- **A02** - Cryptographic Failures
- **A04** - Insecure Design
- **A05** - Security Misconfiguration
- **A07** - Identification and Authentication Failures
- **A08** - Software and Data Integrity Failures
- **A09** - Security Logging and Monitoring Failures
- **A10** - Server-Side Request Forgery (SSRF)

## Troubleshooting

### Database connection fails

```bash
# Check if postgres is running
docker-compose ps

# Check logs
docker-compose logs postgres

# Restart postgres
docker-compose restart postgres
```

### Application won't start

```bash
# Check build errors
docker-compose logs webapp

# Rebuild image
docker-compose up -d --build webapp

# Check if port 5000 is already in use
lsof -i :5000
```

### Changes not reflecting

```bash
# Rebuild Docker image
docker-compose up -d --build webapp

# Or for local development, restart dotnet watch
```

## Security Reminders

1. **NEVER** deploy this application to production
2. **NEVER** expose it publicly on the internet
3. **ALWAYS** run in isolated, local environment
4. **DO NOT** fix vulnerabilities unless explicitly asked
5. **REMEMBER** this is for educational purposes only
