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

> 💡 **Porta 5432 ocupada?** Se você já tiver um PostgreSQL rodando na máquina, o `up` falha com `port is already allocated`. Publique o banco em outra porta com um `docker-compose.override.yml` local (`ports: ["55432:5432"]`); a aplicação fala com o banco pela rede interna e não muda nada.

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
- ✅ Dados de exemplo inseridos (8 produtos, 5 usuários)
- ✅ Permissões configuradas
- ✅ Validação automática executada

Os scripts de inicialização estão em `database/init/` e são executados automaticamente na primeira vez que o container PostgreSQL é criado. Para mais detalhes, veja [database/README.md](database/README.md).

## 📖 Labs Disponíveis

O quadro abaixo é a **fonte da verdade** sobre o que já existe e o que ainda está planejado. Cada lab implementado tem **modo vulnerável e modo seguro** com toggle interativo na própria página.

### ✅ Implementados

| Categoria OWASP | Lab | Página | Documentação |
|-----------------|-----|--------|--------------|
| A01:2021 - Broken Access Control | Elevação de privilégios e IDOR | `/A01-BrokenAccessControl/BrokenAccessControl` | [A01-Broken-Access-Control.md](./docs/labs/A01-Broken-Access-Control.md) |
| A03:2021 - Injection | SQL Injection | `/A03-Injection/SqlInjection` | [A03-SQL-Injection.md](./docs/labs/A03-SQL-Injection.md) |
| A03:2021 - Injection | Prompt Injection (IA) ⚠️ requer chave OpenAI | `/A03-Injection/PromptInjection` | (em elaboração) |
| A09:2021 - Security Logging and Monitoring Failures | Falhas de logging e auditoria | `/A09-LoggingFailures/LoggingFailures` | [A09-Logging-Failures.md](./docs/labs/A09-Logging-Failures.md) |

> ⚠️ **Prompt Injection exige uma chave da OpenAI.** A página `/A03-Injection/PromptInjection` chama a API da OpenAI (modelo `gpt-4o-mini`) e **não funciona sem uma `OPENAI_API_KEY` válida**: sem a chave, a página carrega mas nenhuma análise é executada. Por depender de um modelo pago e externo, **essa página não é usada na disciplina**, que trabalha apenas com modelo local. As demais páginas (A01, A03 SQL e A09) rodam 100% offline, só com os containers do `docker-compose`.

### 🚧 Planejados (ainda sem página)

- **A02:2021** - Cryptographic Failures
- **A04:2021** - Insecure Design
- **A05:2021** - Security Misconfiguration
- **A06:2021** - Vulnerable and Outdated Components
- **A07:2021** - Identification and Authentication Failures
- **A08:2021** - Software and Data Integrity Failures
- **A10:2021** - Server-Side Request Forgery (SSRF)

## 🎓 Uso na Disciplina

Este repositório é a **trilha de ataque** da disciplina Microservice and Web Engineering (FIAP), acervo [`josercf/FIAP-2026-2-3SI`](https://github.com/josercf/FIAP-2026-2-3SI). A ideia é o aluno **explorar a vulnerabilidade real aqui** antes de aprender a defesa no laboratório da aula:

- **Aula 14** (OAuth 2.0, OIDC, JWT e RBAC) usa o lab **A01 - Broken Access Control** ([roteiro](./docs/labs/A01-Broken-Access-Control.md)). O aluno quebra o controle de acesso trocando o identificador do alvo, e só então implementa o 403 que mata esse ataque.
- **Aula 15** (OWASP Top 10 para LLM) usa o lab **A03 - SQL Injection** ([roteiro](./docs/labs/A03-SQL-Injection.md)). O aluno explora a injeção clássica antes de ver o mesmo mecanismo atacar um modelo de linguagem, tornando concreta uma analogia que antes era só afirmada.
- **A09 - Security Logging and Monitoring Failures** ([roteiro](./docs/labs/A09-Logging-Failures.md)) tem roteiro pronto e **ainda não está amarrado a uma aula**. Serve como estudo complementar e é candidato natural para o Checkpoint 3.

A decisão de manter este repositório apartado está registrada na [`ADR-010`](https://github.com/josercf/FIAP-2026-2-3SI/blob/main/docs/adrs/ADR-010-owasp-security-lab-como-trilha-de-ataque.md) do acervo. Ele roda **separado de propósito**: por conter vulnerabilidades intencionais, fica em repositório próprio, isolado, e nunca é publicado junto com o material didático. O aluno sobe o ambiente localmente, ataca no modo vulnerável e observa a correção no modo seguro.

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
│       │   ├── A01-BrokenAccessControl/  # Lab Broken Access Control
│       │   ├── A03-Injection/      # Labs SQL Injection e Prompt Injection
│       │   └── A09-LoggingFailures/      # Lab Logging Failures
│       ├── Services/               # Serviços de negócio (versões Vulnerável + Segura)
│       │   ├── AccessControl/      # Serviços do lab A01
│       │   ├── Database/           # Serviços do lab A03 (SQL)
│       │   ├── Logging/            # Serviços do lab A09
│       │   └── OpenAI/             # Serviços do lab A03 (Prompt)
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
- **5 usuários** com senhas em texto plano (para demonstração): 1 administrador e 4 usuários comuns (um deles inativo)

**Credenciais de Teste**:
- admin@owasp-lab.local / admin123 (Admin)
- john.doe@owasp-lab.local / password123 (User)

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
