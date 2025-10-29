# Database Initialization

## 📋 Como Funciona

O PostgreSQL executa automaticamente todos os scripts SQL em `/docker-entrypoint-initdb.d` quando o container é iniciado **pela primeira vez**.

### Ordem de Execução

Os scripts são executados em ordem alfabética:

1. **01-init.sql** - Cria estrutura e dados
   - Cria usuário `vulnapp`
   - Define permissões
   - Cria tabelas `Product` e `Users`
   - Insere dados de exemplo

2. **99-validation.sql** - Valida a carga
   - Verifica tabelas criadas
   - Conta registros inseridos
   - Lista permissões
   - Mostra logs de confirmação

### Variáveis de Ambiente

Definidas no `docker-compose.yml`:

```yaml
POSTGRES_USER: postgres          # Superusuário
POSTGRES_PASSWORD: postgres      # Senha do superusuário
POSTGRES_DB: vulnerabledb        # Banco criado automaticamente
```

### Usuários do Banco

1. **postgres / postgres** - Superusuário (admin)
2. **vulnapp / password123** - Usuário da aplicação (acesso às tabelas)

## 🔄 Recriar o Banco de Dados

Para forçar a re-execução dos scripts de inicialização:

```bash
# Parar e remover volumes
docker-compose down -v

# Iniciar novamente (irá recriar tudo)
docker-compose up -d
```

⚠️ **ATENÇÃO**: O comando `-v` remove TODOS os dados! Use apenas se quiser um reset completo.

## 🔍 Verificar Logs de Inicialização

```bash
# Ver logs do banco durante inicialização
docker-compose logs postgres

# Ver logs em tempo real
docker-compose logs -f postgres
```

Você deverá ver as mensagens de validação do script `99-validation.sql`.

## 🛠️ Estrutura de Dados

### Tabela: Product

| Column | Type | Description |
|--------|------|-------------|
| Id | VARCHAR(120) | Primary Key |
| Name | VARCHAR(120) | Product name |
| Price | VARCHAR(120) | Product price |

**8 produtos** pré-carregados (Hammer, Drills, Wrench Set, etc.)

### Tabela: Users

| Column | Type | Description |
|--------|------|-------------|
| Id | VARCHAR(120) | Primary Key |
| Name | VARCHAR(120) | Username |
| Password | VARCHAR(120) | Password (plain text for demo!) |

**4 usuários** pré-carregados:
- admin / admin123
- john.doe / password123
- jane.smith / secret456
- bob.wilson / qwerty789

⚠️ **Senhas em texto plano**: Apenas para demonstração educacional de SQL Injection!

## 🔐 Healthcheck

O docker-compose inclui um healthcheck que verifica:

1. ✅ PostgreSQL está respondendo (`pg_isready`)
2. ✅ Banco `vulnerabledb` está acessível
3. ✅ Tabela `Product` foi criada e tem dados

A aplicação web só inicia quando o healthcheck passar!

```yaml
healthcheck:
  test: ["CMD-SHELL", "pg_isready -U postgres -d vulnerabledb && psql -U postgres -d vulnerabledb -c 'SELECT COUNT(*) FROM \"Product\";' > /dev/null 2>&1"]
  interval: 10s
  timeout: 5s
  retries: 10
  start_period: 30s
```

## 🧪 Testar Conexão Manual

```bash
# Conectar ao banco via docker exec
docker exec -it owasp-lab-db psql -U postgres -d vulnerabledb

# Ou como usuário da aplicação
docker exec -it owasp-lab-db psql -U vulnapp -d vulnerabledb

# Verificar dados
vulnerabledb=# SELECT COUNT(*) FROM "Product";
vulnerabledb=# SELECT COUNT(*) FROM "Users";
vulnerabledb=# \dt  -- Listar tabelas
vulnerabledb=# \q   -- Sair
```

## 📝 Adicionar Novos Scripts

Para adicionar mais dados ou estruturas:

1. Crie um novo arquivo SQL: `02-new-data.sql`
2. Coloque em `database/init/`
3. Será executado automaticamente na próxima recriação

**Convenção de nomes**:
- `01-` a `98-` - Scripts de setup
- `99-` - Scripts de validação
