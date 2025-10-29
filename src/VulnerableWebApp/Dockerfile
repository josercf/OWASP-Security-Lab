# ========================================
# OWASP Security Lab - Dockerfile
# ========================================
# Build with .NET 10 SDK

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copiar csproj e restaurar dependências
COPY ["VulnerableWebApp.csproj", "./"]
RUN dotnet restore "VulnerableWebApp.csproj"

# Copiar todo o código e fazer build
COPY . .
RUN dotnet build "VulnerableWebApp.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "VulnerableWebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app

# Criar usuário não-root para segurança
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copiar arquivos publicados
COPY --from=publish /app/publish .

# Mudar ownership dos arquivos
RUN chown -R appuser:appuser /app

# Usar usuário não-root
USER appuser

# Expor porta
EXPOSE 8080
EXPOSE 8081

# Definir variáveis de ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Comando de inicialização
ENTRYPOINT ["dotnet", "VulnerableWebApp.dll"]
