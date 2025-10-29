# 🔐 OWASP Security Lab

Laboratório educacional de vulnerabilidades web baseado no **OWASP Top 10 - 2021**.

> ⚠️ **AVISO IMPORTANTE**: Este projeto contém **INTENCIONALMENTE** vulnerabilidades de segurança para fins educacionais. **NUNCA** utilize este código em ambiente de produção ou exponha-o publicamente na internet!

## 📚 Sobre o Projeto

O OWASP Security Lab é uma aplicação web desenvolvida em ASP.NET Core (.NET 10) que demonstra as principais vulnerabilidades de segurança em aplicações web conforme classificadas pela [OWASP Top 10 - 2021](https://owasp.org/Top10/).

### Objetivos

- 🎓 **Educacional**: Ensinar desenvolvedores sobre vulnerabilidades comuns
- 🔍 **Prático**: Demonstrar ataques reais em ambiente controlado
- 🛡️ **Defensivo**: Mostrar como corrigir e prevenir cada vulnerabilidade
- 🚀 **Acessível**: Executar todo o ambiente com um único comando Docker

## 🏗️ Tecnologias

- **.NET 10** (RC) - Framework web
- **ASP.NET Core Razor Pages** - Frontend
- **PostgreSQL 16** - Banco de dados
- **Docker & Docker Compose** - Containerização
- **Bootstrap 5** - Interface

## 🚀 Início Rápido

### Pré-requisitos

- [Docker](https://www.docker.com/get-started) (versão 20.10+)
- [Docker Compose](https://docs.docker.com/compose/install/) (versão 2.0+)

### Executando o Lab

1. **Clone o repositório**
   ```bash
   git clone https://github.com/josercf/OWASP-Security-Lab.git
   cd OWASP-Security-Lab
   ```

2. **Inicie os containers**
   ```bash
   docker-compose up -d
   ```

3. **Acesse a aplicação**
   - 🌐 **Web App**: http://localhost:8080
   - 🗄️ **PgAdmin** (opcional): http://localhost:5050
     - Email: `admin@owasplab.local`
     - Senha: `admin123`

4. **Pare os containers**
   ```bash
   docker-compose down
   ```

### Logs e Debug

```bash
# Ver logs da aplicação
docker-compose logs -f webapp

# Ver logs do banco de dados (inclui logs de inicialização)
docker-compose logs -f postgres

# Reiniciar apenas a aplicação
docker-compose restart webapp
```

### Reset Completo do Banco de Dados

Para recriar o banco com dados limpos:

```bash
# Para e remove TODOS os dados (volumes)
docker-compose down -v

# Inicia novamente (recria tudo do zero)
docker-compose up -d
```

⚠️ O banco de dados é inicializado automaticamente com:
- ✅ Tabelas criadas (`Product`, `Users`)
- ✅ Dados de exemplo inseridos (8 produtos, 4 usuários)
- ✅ Permissões configuradas
- ✅ Validação automática executada

Os scripts de inicialização estão em `database/init/` e são executados automaticamente na primeira vez que o container PostgreSQL é criado. Para mais detalhes, veja [database/README.md](database/README.md).

## 📖 Labs Disponíveis

### ✅ A03:2021 - Injection (SQL Injection)

**Status**: Disponível
**Página**: `/A03-Injection/SqlInjection`

Demonstra vulnerabilidades de SQL Injection através de consultas não parametrizadas.

**Objetivos de Aprendizado**:
- Entender como ataques de SQL Injection funcionam
- Explorar diferentes payloads de injeção
- Comparar código vulnerável vs. código seguro
- Aprender sobre consultas parametrizadas

**Payloads de Teste**:
```sql
' OR '1'='1
' OR 1=1 --
' UNION SELECT "Id","Name","Password" FROM "Users" --
```

**Documentação**: [A03-SQL-Injection.md](./docs/labs/A03-SQL-Injection.md)

### 🚧 Próximos Labs

- **A01:2021** - Broken Access Control
- **A02:2021** - Cryptographic Failures
- **A04:2021** - Insecure Design
- **A05:2021** - Security Misconfiguration
- **A07:2021** - Identification and Authentication Failures
- **A08:2021** - Software and Data Integrity Failures
- **A09:2021** - Security Logging and Monitoring Failures
- **A10:2021** - Server-Side Request Forgery (SSRF)

## 🛠️ Desenvolvimento Local

### Sem Docker

Se você preferir executar localmente sem Docker:

1. **Instale o .NET 10 SDK**
   ```bash
   # Baixe em: https://dotnet.microsoft.com/download/dotnet/10.0
   dotnet --version  # Deve mostrar 10.0.x
   ```

2. **Configure o PostgreSQL**
   ```bash
   # Inicie apenas o banco de dados
   docker-compose up -d postgres
   ```

3. **Execute a aplicação**
   ```bash
   cd src/VulnerableWebApp
   dotnet restore
   dotnet run
   ```

4. **Acesse**
   - http://localhost:8080 (ou a porta indicada no console)

### Build Manual

```bash
# Build da aplicação
cd src/VulnerableWebApp
dotnet build

# Build da imagem Docker
docker build -t owasp-lab:latest .
```

## 📁 Estrutura do Projeto

```
OWASP-Security-Lab/
├── src/
│   └── VulnerableWebApp/          # Aplicação ASP.NET Core
│       ├── Models/                 # Modelos de dados
│       ├── Pages/                  # Razor Pages
│       │   ├── Index.cshtml        # Dashboard principal
│       │   └── A03-Injection/      # Lab SQL Injection
│       ├── Services/               # Serviços de negócio
│       │   └── Database/           # Serviços de banco
│       ├── Program.cs              # Configuração da aplicação
│       └── Dockerfile              # Dockerfile da aplicação
├── database/
│   └── init/
│       └── 01-init.sql             # Script de inicialização do DB
├── docs/
│   └── labs/                       # Documentação dos labs
├── docker-compose.yml              # Orquestração Docker
├── CLAUDE.md                       # Guia para Claude Code
└── README.md                       # Este arquivo
```

## 🔒 Segurança

### Avisos Importantes

1. ⛔ **NÃO EXPONHA PUBLICAMENTE**: Este lab contém vulnerabilidades intencionais
2. 🔒 **USE APENAS LOCALMENTE**: Execute apenas em ambiente isolado
3. 🎓 **FIM EDUCACIONAL**: Destinado exclusivamente para aprendizado
4. 🚫 **NÃO USE EM PRODUÇÃO**: Código vulnerável por design

### Dados de Teste

O banco de dados é inicializado com:
- **8 produtos** de exemplo
- **4 usuários** com senhas em texto plano (para demonstração)

**Credenciais de Teste**:
- admin / admin123
- john.doe / password123

## 📚 Recursos Adicionais

- [OWASP Top 10 - 2021](https://owasp.org/Top10/)
- [OWASP Cheat Sheet Series](https://cheatsheetseries.owasp.org/)
- [OWASP WebGoat](https://owasp.org/www-project-webgoat/)
- [PortSwigger Web Security Academy](https://portswigger.net/web-security)

## 🤝 Contribuindo

Contribuições são bem-vindas! Para adicionar novos labs:

1. Fork o repositório
2. Crie uma branch para sua feature (`git checkout -b feature/A01-Lab`)
3. Commit suas mudanças (`git commit -am 'Add A01 lab'`)
4. Push para a branch (`git push origin feature/A01-Lab`)
5. Abra um Pull Request

## 📝 Licença

Este projeto é licenciado sob a [MIT License](LICENSE).

## 👨‍💻 Autor

**José Costa**
- GitHub: [@josercf](https://github.com/josercf)

## 🙏 Agradecimentos

- [OWASP Foundation](https://owasp.org/) - Pela classificação e documentação de vulnerabilidades
- [Microsoft](https://dotnet.microsoft.com/) - Pelo framework .NET
- Comunidade de segurança da informação

---

**⚠️ Lembre-se**: Use este projeto apenas para fins educacionais em ambientes controlados!
