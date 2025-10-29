# A03:2021 - Injection (SQL Injection)

## 🎯 Objetivo do Lab

Aprender sobre vulnerabilidades de SQL Injection, como explorá-las e, mais importante, como preveni-las.

## 📖 O que é SQL Injection?

SQL Injection é uma vulnerabilidade que ocorre quando dados fornecidos pelo usuário são concatenados diretamente em consultas SQL sem validação ou sanitização adequada. Isso permite que um atacante manipule a consulta SQL para:

- Bypassar autenticação
- Ler dados sensíveis
- Modificar ou deletar dados
- Executar comandos administrativos no banco de dados

### Classificação OWASP

- **Categoria**: A03:2021 - Injection
- **CWE**: CWE-89 (SQL Injection)
- **Risco**: 🔴 Alto
- **Prevalência**: Comum
- **Impacto**: Crítico

## 🔍 Como Funciona

### Código Vulnerável

```csharp
// ⚠️ VULNERÁVEL - NÃO FAÇA ISSO!
cmd.CommandText = $@"SELECT * FROM ""Product"" WHERE ""Name"" LIKE '%{searchString}%'";
```

Quando o usuário digita: `Hammer`

A query executada é:
```sql
SELECT * FROM "Product" WHERE "Name" LIKE '%Hammer%'
```

Mas se o usuário digita: `' OR '1'='1`

A query executada se torna:
```sql
SELECT * FROM "Product" WHERE "Name" LIKE '%' OR '1'='1%'
```

Isso sempre retorna `TRUE`, expondo todos os produtos!

### Código Seguro

```csharp
// ✅ SEGURO - Use consultas parametrizadas
cmd.CommandText = @"SELECT * FROM ""Product"" WHERE ""Name"" LIKE @searchString";
cmd.Parameters.AddWithValue("@searchString", $"%{searchString}%");
```

## 🧪 Experimentos

### Nível 1: Básico - Bypass de Filtro

**Objetivo**: Retornar todos os produtos ao invés de apenas os correspondentes.

**Payload**:
```sql
' OR '1'='1
```

**O que acontece**:
- A condição `'1'='1'` é sempre verdadeira
- Todos os produtos são retornados

**Query resultante**:
```sql
SELECT * FROM "Product" WHERE "Name" LIKE '%' OR '1'='1%'
```

### Nível 2: Comentários - Ignorar Resto da Query

**Objetivo**: Usar comentários SQL para ignorar o resto da query.

**Payload**:
```sql
' OR 1=1 --
```

**O que acontece**:
- `--` é o comentário de linha no SQL
- Tudo após `--` é ignorado
- A query retorna todos os registros

**Query resultante**:
```sql
SELECT * FROM "Product" WHERE "Name" LIKE '%' OR 1=1 --%'
```

### Nível 3: UNION - Exfiltração de Dados

**Objetivo**: Combinar resultados de outra tabela (Users) com a consulta original.

**Payload**:
```sql
' UNION SELECT "Id","Name","Password" FROM "Users" --
```

**O que acontece**:
- `UNION` combina resultados de duas consultas
- Expõe dados da tabela Users
- Mostra senhas em texto plano

**Query resultante**:
```sql
SELECT * FROM "Product" WHERE "Name" LIKE '%'
UNION SELECT "Id","Name","Password" FROM "Users" --%'
```

### Nível 4: Stacked Queries (Tentativa de Dano)

**Objetivo**: Tentar executar múltiplos comandos SQL.

**Payload**:
```sql
'; DROP TABLE "Product"; --
```

**O que acontece**:
- Em alguns SGBDs, isso executaria DROP TABLE
- No PostgreSQL com Npgsql, isso geralmente falha (proteção do driver)
- Mas demonstra o potencial destrutivo

### Nível 5: Boolean-Based Blind

**Objetivo**: Extrair informações quando não há output direto.

**Payload**:
```sql
' AND (SELECT COUNT(*) FROM "Users") > 0 --
```

**O que acontece**:
- Retorna resultados se a condição for verdadeira
- Permite inferir informações sobre o banco
- Base para ataques blind

## 🛡️ Como Prevenir

### 1. Consultas Parametrizadas (Recommended)

```csharp
// ✅ Sempre use parâmetros
cmd.CommandText = @"SELECT * FROM ""Product"" WHERE ""Name"" LIKE @searchString";
cmd.Parameters.AddWithValue("@searchString", $"%{searchString}%");
```

### 2. ORM (Entity Framework, Dapper)

```csharp
// ✅ ORMs geralmente protegem automaticamente
var products = context.Products
    .Where(p => p.Name.Contains(searchString))
    .ToList();
```

### 3. Validação de Input

```csharp
// ✅ Valide e sanitize inputs
if (!Regex.IsMatch(searchString, @"^[a-zA-Z0-9\s-]+$"))
{
    throw new ArgumentException("Invalid search string");
}
```

### 4. Princípio do Menor Privilégio

```sql
-- ✅ Usuário do app não deve ter permissões administrativas
GRANT SELECT, INSERT, UPDATE ON Product TO vulnapp;
-- NÃO dar DROP, CREATE, etc.
```

### 5. Prepared Statements

```csharp
// ✅ Use prepared statements
using var cmd = new NpgsqlCommand();
cmd.Prepare();
cmd.Parameters.AddWithValue("@param", value);
```

## 📊 Comparação

| Aspecto | Vulnerável | Seguro |
|---------|------------|--------|
| Método | String concatenation | Parametrized queries |
| Risco | 🔴 Crítico | 🟢 Baixo |
| Performance | Similar | Melhor (cached) |
| Manutenção | Difícil | Fácil |
| Teste | Complexo | Simples |

## 🎓 Exercícios

### Exercício 1: Identifique Vulnerabilidades

Analise o código em `VulnerableDatabaseService.cs` e identifique:
1. Onde está a vulnerabilidade exata
2. Que tipos de ataque são possíveis
3. Quais dados podem ser comprometidos

### Exercício 2: Teste os Payloads

Execute todos os payloads listados acima e observe:
1. A query SQL executada (mostrada na interface)
2. Os resultados retornados
3. Possíveis mensagens de erro

### Exercício 3: Compare Implementações

Compare `VulnerableDatabaseService.cs` com `SecureDatabaseService.cs`:
1. Identifique as diferenças
2. Entenda como a parametrização funciona
3. Por que ela previne SQL Injection

### Exercício 4: Crie Seus Próprios Payloads

Tente criar payloads para:
1. Descobrir quantos usuários existem
2. Descobrir o nome das tabelas
3. Extrair a estrutura do banco

## 📚 Recursos Adicionais

### Leitura Obrigatória

- [OWASP SQL Injection](https://owasp.org/www-community/attacks/SQL_Injection)
- [OWASP SQL Injection Prevention Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/SQL_Injection_Prevention_Cheat_Sheet.html)
- [CWE-89: SQL Injection](https://cwe.mitre.org/data/definitions/89.html)

### Ferramentas

- [SQLMap](https://sqlmap.org/) - Automated SQL injection tool
- [Burp Suite](https://portswigger.net/burp) - Web security testing
- [OWASP ZAP](https://www.zaproxy.org/) - Security scanner

### Prática Adicional

- [PortSwigger SQL Injection Labs](https://portswigger.net/web-security/sql-injection)
- [HackTheBox](https://www.hackthebox.com/)
- [TryHackMe](https://tryhackme.com/)

## ⚠️ Avisos Finais

1. **Ambiente Controlado**: Use este lab apenas em ambiente local isolado
2. **Fins Educacionais**: O conhecimento é para defesa, não ataque
3. **Responsabilidade**: Não teste em sistemas sem autorização explícita
4. **Ética**: Use o conhecimento adquirido de forma ética e legal

## 🔄 Próximos Passos

Após dominar SQL Injection:
1. Explore outros tipos de Injection (XSS, Command Injection, LDAP)
2. Aprenda sobre WAFs (Web Application Firewalls)
3. Estude técnicas de bypass avançadas
4. Implemente validação em múltiplas camadas

---

**Lembre-se**: A melhor defesa é nunca confiar em dados do usuário! 🛡️
