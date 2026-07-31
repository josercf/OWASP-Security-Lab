# A09:2021 - Security Logging and Monitoring Failures

## 🎯 Objetivo do Lab

Entender por que a **ausência** de logs de segurança é, ela própria, uma vulnerabilidade. Você vai executar ataques que passam **sem deixar rastro** no modo vulnerável e ver os mesmos eventos serem **registrados com contexto completo** e persistidos no banco no modo seguro.

> Este roteiro acompanha a **Aula 15** da disciplina Microservice and Web Engineering (acervo `josercf/FIAP-2026-2-3SI`). Aqui você comprova que "não ter log" é o mesmo que "não ver o ataque"; na aula você aprende a instrumentar a defesa.

## 📖 O que são falhas de Logging e Monitoramento?

Diferente de SQL Injection ou Broken Access Control, esta categoria não é uma porta de entrada: é a **falta de visibilidade**. Se um ataque acontece e nada é registrado, ninguém detecta, ninguém investiga e ninguém responde. O invasor pode tentar indefinidamente.

As falhas típicas que este lab demonstra:

- **Logs vagos e inúteis**: "User logged in" sem dizer quem, quando ou de onde.
- **Eventos críticos ignorados**: falhas de login e tentativas de acesso não autorizado simplesmente não são registradas.
- **Sem persistência**: log só no console, perdido quando o processo reinicia; nada vai para um armazenamento auditável.
- **Sem contexto**: falta usuário, IP, timestamp preciso, valor antes/depois de uma alteração.
- **Sem alerta**: mesmo quando algo é registrado, nada dispara notificação para a equipe de segurança.

### Classificação OWASP

- **Categoria**: A09:2021 - Security Logging and Monitoring Failures
- **CWE**: CWE-778 (Insufficient Logging), CWE-223 (Omission of Security-relevant Information), CWE-532 (Insertion of Sensitive Information into Log File)
- **Risco**: 🟠 Médio a Alto (amplifica o impacto de todas as outras categorias)
- **Prevalência**: Comum
- **Impacto**: Detecção e resposta comprometidas; investigação forense impossível; violação de compliance (LGPD, GDPR, PCI-DSS)

### O que um bom log de segurança precisa responder

| Pergunta | Campo | No modo vulnerável | No modo seguro |
|----------|-------|--------------------|-----------------|
| Quem? | Username | ausente | registrado |
| O quê? | EventType / Action | genérico | específico |
| Quando? | Timestamp (UTC) | ausente | preciso |
| De onde? | IpAddress | ausente | registrado |
| Deu certo? | Success | ausente | registrado |
| Persistiu? | tabela AuditLog | não | sim |

## 🔍 Como Funciona

A página `/A09-LoggingFailures/LoggingFailures` simula cinco eventos de segurança (tentativa de login, acesso a dados, acesso não autorizado, modificação de dados e atividade suspeita) e mostra, ao lado, uma tabela com os **logs de auditoria** lidos da tabela `AuditLog` do PostgreSQL.

### Código Vulnerável

`Services/Logging/VulnerableLoggingService.cs`

```csharp
public LoggingResult SimulateLoginAttempt(string username, string password, string ip, bool success)
{
    // ⚠️ Log genérico, sem contexto, e SÓ para sucesso
    if (success)
        _logger.LogInformation("User logged in");   // não diz quem, quando, de onde
    // ⚠️ Falhas de login NÃO são registradas de forma alguma
    // ⚠️ Nada é persistido no banco
}

public LoggingResult SimulateUnauthorizedAccess(string username, string resource, string ip)
{
    // ⚠️ VULNERABILIDADE CRÍTICA: silêncio total, nenhum log
}
```

A tentativa de acesso não autorizado **não gera nenhuma linha de log** e **nada** vai para o banco.

### Código Seguro

`Services/Logging/SecureLoggingService.cs`

```csharp
public LoggingResult SimulateUnauthorizedAccess(string username, string resource, string ip)
{
    var log = new AuditLog
    {
        Timestamp = DateTime.UtcNow,
        Level = "Critical",
        EventType = "UnauthorizedAccess",
        Username = username,
        IpAddress = ip,
        Success = false,
        Message = $"⚠️ SECURITY ALERT: '{username}' tentou acesso não autorizado a '{resource}'",
        Details = JsonSerializer.Serialize(new { Severity = "High", AlertSent = true })
    };
    SaveAuditLog(log);                        // ✅ persiste no PostgreSQL
    _logger.LogCritical("🚨 SECURITY ALERT | {Event} | {User} | {Ip}", ...);
}
```

O evento é estruturado, classificado como `Critical`, persistido na tabela `AuditLog` e gera um log crítico na aplicação. Isso é o que torna o ataque **detectável**.

## 🧪 Experimentos

Suba o ambiente:

```bash
docker-compose up -d
# acesse http://localhost:8080/A09-LoggingFailures/LoggingFailures
```

O botão **🔓 Vulnerável / 🔒 Seguro** troca o modo. A tabela "Logs de Auditoria" na página lê a tabela `AuditLog`; nos experimentos vamos usá-la para provar o que foi (ou não foi) registrado. Se quiser inspecionar o banco diretamente:

```bash
docker exec owasp-lab-db psql -U postgres -d vulnerabledb -c 'SELECT COUNT(*) FROM "AuditLog";'
```

### Nível 1: O ataque que não deixa rastro

**Objetivo**: executar uma tentativa de acesso não autorizado no modo vulnerável e comprovar que nada foi registrado.

**Passos** (modo **Vulnerável**, `?IsSecureMode=false`):
1. Se houver logs antigos, clique em **🗑️ Limpar Logs** para partir do zero.
2. Clique em **⚠️ Tentar Acesso Não Autorizado** (bloco 3 da página).

**Resultado esperado**:
- O painel de resultado mostra: `(SEM LOG - Silêncio total!)`.
- A tabela de auditoria continua vazia.
- No banco, `SELECT COUNT(*) FROM "AuditLog"` continua **0**.

O invasor tentou acessar o painel administrativo e a organização não tem **nenhum** registro disso.

> ✅ **Validado** contra o container: após a tentativa no modo vulnerável, a tabela `AuditLog` permaneceu com 0 linhas e o resultado exibido foi "SEM LOG - Silêncio total!".

### Nível 2: Falha de login que ninguém vê (força bruta invisível)

**Objetivo**: mostrar que falhas de login não são registradas, o que torna um ataque de força bruta indetectável.

**Passos** (modo **Vulnerável**):
1. No topo, mantenha um `Username` qualquer (ex.: `attacker`).
2. Clique em **❌ Login Falha** algumas vezes (simulando várias tentativas).

**Resultado esperado**:
- O resultado exibido é `(Sem log)`.
- Nenhuma linha aparece na tabela de auditoria; o banco continua em 0.

Cem tentativas de senha erradas produziriam exatamente zero evidências.

> ✅ **Validado**: uma tentativa `LoginAttempt` com `success=false` no modo vulnerável não gravou nenhuma linha em `AuditLog`.

### Nível 3: Ligando o modo seguro - o mesmo ataque agora é visível

**Objetivo**: repetir os eventos com o logging seguro e ver cada um ser registrado com contexto completo.

**Passos**:
1. Clique em **🔒 Seguro** (a URL passa a `?IsSecureMode=true`).
2. Clique em **⚠️ Tentar Acesso Não Autorizado**.

**Resultado esperado**:
- O painel mostra um **ALERTA DE SEGURANÇA** de nível `Critical` com usuário, recurso, IP e timestamp.
- Uma nova linha aparece na tabela de auditoria (vermelha, nível Critical).
- No banco, `SELECT COUNT(*) FROM "AuditLog"` passa a **1**, com `Level = Critical`, `EventType = UnauthorizedAccess`, o `Username` e o `IpAddress` reais.

3. Clique também em **❌ Login Falha** e confirme que agora surge uma linha `Warning` de evento `Login` com `Success = false`.

> ✅ **Validado**: no modo seguro, a tentativa de acesso não autorizado gravou uma linha `Critical` em `AuditLog` (com usuário e IP `::ffff:192.168.65.1` capturado do request), e a falha de login gravou uma linha `Warning`.

### Nível 4: Contexto para investigação (antes/depois)

**Objetivo**: ver que uma alteração de dados registra o valor anterior e o novo, permitindo auditoria e reversão.

**Passos** (modo **Seguro**):
1. No bloco 4 (**Data Modification**), os campos já vêm preenchidos com `role=user` (valor anterior) e `role=admin` (novo valor), uma escalada de privilégio típica.
2. Clique em **📝 Modificar Dados**.

**Resultado esperado**:
- O log seguro mostra `Old Value` e `New Value`.
- A linha persistida guarda esse contexto em `Details` (JSON), permitindo reconstruir exatamente o que mudou.

Faça o mesmo no modo **Vulnerável** e note que o log é apenas "Data changed", sem dizer o quê nem por quem.

### Nível 5: Comparação lado a lado

**Objetivo**: consolidar a diferença.

1. Em cada modo, dispare os cinco eventos (login sucesso/falha, acesso a dados, acesso não autorizado, modificação, atividade suspeita).
2. Compare a tabela de auditoria nos dois modos:
   - **Vulnerável**: permanece vazia ou quase vazia; o banco não recebe os eventos.
   - **Seguro**: cada evento vira uma linha com nível apropriado (Info, Warning, Critical), usuário, IP e mensagem descritiva.

## 🛡️ Como Prevenir

### 1. Registre todos os eventos de segurança
Login (sucesso **e** falha), logout, falhas de autorização, acesso a dados sensíveis, alterações de privilégio e entradas inválidas. O que não é registrado não pode ser detectado.

### 2. Log estruturado e com contexto
Cada evento deve responder quem, o quê, quando, de onde e com que resultado. Prefira formato estruturado (JSON) para permitir busca e correlação, como o campo `Details` do `AuditLog` faz aqui.

### 3. Persista em armazenamento auditável
Console não é auditoria. Envie os logs para um destino durável e protegido (banco, SIEM, storage append-only), com retenção e controle de integridade.

### 4. Nunca registre segredos
Log é para contexto, não para senhas, tokens ou dados de cartão (CWE-532). Registre o **fato** ("falha de login do usuário X"), não a credencial digitada.

### 5. Monitore e alerte
Log parado num arquivo não defende ninguém. Configure alertas para padrões suspeitos (múltiplas falhas de login, picos de acesso negado) e correlacione eventos, como o modo seguro sinaliza com nível `Critical`.

### 6. Amarre com as outras categorias
Cada ACESSO NEGADO do lab A01 deveria virar uma linha de auditoria aqui. Logging é a camada que dá visibilidade a todos os outros ataques.

## 📊 Comparação

| Aspecto | Vulnerável | Seguro |
|---------|------------|--------|
| Tentativa de acesso não autorizado | Silêncio total (0 linhas) | Linha `Critical` persistida |
| Falha de login | Não registrada | Linha `Warning` persistida |
| Persistência | Só console (ou nada) | Tabela `AuditLog` no PostgreSQL |
| Contexto (quem/quando/onde) | Ausente | Completo, inclusive JSON em `Details` |
| Detecção de força bruta | Impossível | Viável por correlação |
| Compliance (LGPD/GDPR/PCI) | Violado | Atendido |

## 🎓 Exercícios

### Exercício 1: Prove o silêncio
No modo vulnerável, limpe os logs, dispare uma tentativa de acesso não autorizado e uma falha de login, depois rode `SELECT COUNT(*) FROM "AuditLog";`. Explique por que o resultado é 0 e o que isso significa para uma equipe de resposta a incidentes.

### Exercício 2: Reconstrua o incidente
No modo seguro, dispare os cinco eventos. A partir apenas das linhas da tabela `AuditLog`, escreva a "história" do que aconteceu: quem, de qual IP, tentando o quê e com qual resultado.

### Exercício 3: Encontre o que NÃO deve ser logado
Analise `SecureLoggingService.SimulateLoginAttempt`. O parâmetro `password` chega ao método. Ele é gravado em algum lugar? Deveria? Relacione com CWE-532.

### Exercício 4: Do log ao alerta
O modo seguro marca eventos como `Critical`. Descreva, em pseudocódigo, uma regra que dispararia um alerta após N falhas de login do mesmo IP em M segundos. Que campos da `AuditLog` você usaria?

### Exercício 5: Ponte para a Aula 15
Explique como você conectaria os logs deste lab a um pipeline de observabilidade (coleta, armazenamento, alerta) para reduzir o tempo de detecção de um ataque.

## 📚 Recursos Adicionais

- [OWASP A09:2021 - Security Logging and Monitoring Failures](https://owasp.org/Top10/A09_2021-Security_Logging_and_Monitoring_Failures/)
- [OWASP Logging Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html)
- [OWASP Logging Vocabulary Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Logging_Vocabulary_Cheat_Sheet.html)
- [CWE-778: Insufficient Logging](https://cwe.mitre.org/data/definitions/778.html)
- [CWE-532: Insertion of Sensitive Information into Log File](https://cwe.mitre.org/data/definitions/532.html)

## ⚠️ Avisos Finais

1. **Ambiente controlado**: use este lab apenas em ambiente local isolado.
2. **Fins educacionais**: o conhecimento é para defesa, não ataque.
3. **Cuidado com o que se registra**: um log bem-intencionado que grava senhas vira, ele próprio, uma vulnerabilidade.

---

**Lembre-se**: um ataque sem log é um ataque invisível. Você não pode responder ao que não consegue ver. 🛡️
