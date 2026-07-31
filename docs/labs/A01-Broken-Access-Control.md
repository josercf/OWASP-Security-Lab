# A01:2021 - Broken Access Control

## 🎯 Objetivo do Lab

Aprender como falhas de controle de acesso permitem que um usuário comum execute ações reservadas a administradores e leia dados de outras contas. Ao final, entender a diferença entre **autenticação** e **autorização**, o papel do **RBAC** (Role-Based Access Control) e por que responder **401** ou **403** corretamente faz parte da defesa.

> Este roteiro acompanha a **Aula 14** da disciplina Microservice and Web Engineering (acervo `josercf/FIAP-2026-2-3SI`). Aqui você explora a vulnerabilidade real; na aula você aprende a defesa.

## 📖 O que é Broken Access Control?

Controle de acesso decide **quem pode fazer o quê**. Ele falha quando a aplicação verifica apenas se o usuário está **autenticado** (fez login) mas não verifica se ele está **autorizado** (tem o papel/permissão necessário) para a ação solicitada.

Duas falhas clássicas aparecem neste lab:

- **Broken Function Level Authorization** (escalada de privilégio): um usuário comum executa uma operação administrativa (deletar usuário, ativar/desativar conta) porque o código nunca checa o papel dele.
- **IDOR - Insecure Direct Object Reference** (acesso a objeto de outra pessoa): o alvo da operação é um identificador (`TargetUserId`) enviado pelo cliente. Como nada valida se aquele objeto pertence ao usuário logado, basta trocar o identificador para agir sobre a conta de qualquer outra pessoa, inclusive a do administrador.

### Classificação OWASP

- **Categoria**: A01:2021 - Broken Access Control (posição nº 1 do OWASP Top 10 2021)
- **CWE**: CWE-284 (Improper Access Control), CWE-285 (Improper Authorization), CWE-639 (Authorization Bypass Through User-Controlled Key / IDOR)
- **Risco**: 🔴 Alto
- **Prevalência**: Muito comum (foi a categoria com mais ocorrências no Top 10 2021)
- **Impacto**: Crítico

### Autenticação x Autorização (vocabulário da Aula 14)

| Pergunta | Conceito | Onde falha aqui |
|----------|----------|-----------------|
| "Quem é você?" | Autenticação (login) | Funciona: o login valida email e senha |
| "O que você pode fazer?" | Autorização (papel / RBAC) | Falha: nenhuma checagem de papel antes da ação |

- **401 Unauthorized**: você **não provou quem é** (não autenticado, ou credencial inválida).
- **403 Forbidden**: você **provou quem é, mas não tem permissão** para o recurso.

No modo vulnerável deste lab, o usuário comum é autenticado (deveria seguir para um **403** ao tentar uma ação de admin), mas a operação simplesmente acontece. No modo seguro, a ação de admin feita por um usuário comum é recusada com uma mensagem equivalente a **403** ("ACESSO NEGADO").

## 🔍 Como Funciona

A página `/A01-BrokenAccessControl/BrokenAccessControl` tem um painel administrativo que lista todos os usuários e oferece três operações sensíveis por usuário: **ver dados sensíveis**, **ativar/desativar** e **deletar**. O alvo de cada operação é enviado no campo `TargetUserId`.

### Código Vulnerável

`Services/AccessControl/VulnerableAccessControlService.cs`

```csharp
public AdminOperation DeleteUser(string userId, UserSession currentUser)
{
    // ⚠️ VULNERABILIDADE: NÃO VALIDA SE O USUÁRIO É ADMIN!
    // Qualquer usuário autenticado pode deletar outros usuários.
    // Também não valida se 'userId' pertence a quem está logado (IDOR).

    // ... vai direto para o DELETE
    cmd.CommandText = @"DELETE FROM ""Users"" WHERE ""Id"" = @userId";
    cmd.Parameters.AddWithValue("@userId", userId);
    var rowsAffected = cmd.ExecuteNonQuery();
    // ...
}
```

A única barreira presente é "estar logado". O papel (`Role`) do usuário nunca entra na decisão.

### Código Seguro

`Services/AccessControl/SecureAccessControlService.cs`

```csharp
private bool IsAdmin(UserSession user)
    => user.IsAuthenticated && user.Role == "Admin";

public AdminOperation DeleteUser(string userId, UserSession currentUser)
{
    // ✅ VALIDAÇÃO: verifica o papel ANTES de executar
    if (!IsAdmin(currentUser))
    {
        _logger.LogWarning("🔒 ACESSO NEGADO - {Email} (Role: {Role}) tentou deletar {Target}",
            currentUser.Email, currentUser.Role, userId);
        return new AdminOperation
        {
            Success = false,
            WasAuthorized = false,
            Message = "❌ ACESSO NEGADO! Apenas administradores podem deletar usuários."
        };
    }
    // ... só chega aqui se for Admin
}
```

A checagem `IsAdmin(currentUser)` é a implementação mínima de RBAC: a operação só prossegue se o papel for `Admin`. Em produção, esse mesmo controle deveria estar em uma camada central (middleware / policy / atributo `[Authorize(Roles = "Admin")]`), e não repetido à mão em cada método, mas o princípio é o mesmo.

## 🧪 Experimentos

Antes de começar, suba o ambiente:

```bash
docker-compose up -d
# aguarde o banco ficar saudável e acesse http://localhost:8080
```

Usuários semeados pelo banco (`database/init/01-init.sql`):

| Id | Nome | Email | Senha | Role | Ativo |
|----|------|-------|-------|------|-------|
| 1 | Administrator | admin@owasp-lab.local | admin123 | Admin | sim |
| 2 | John Doe | john.doe@owasp-lab.local | password123 | User | sim |
| 3 | Jane Smith | jane.smith@owasp-lab.local | secret456 | User | sim |
| 4 | Bob Wilson | bob.wilson@owasp-lab.local | qwerty789 | User | sim |
| 5 | Alice Johnson | alice.johnson@owasp-lab.local | pass9999 | User | não |

O botão **🔓 Vulnerável / 🔒 Seguro** no topo da página troca o modo (parâmetro `IsSecureMode` na URL).

### Nível 1: Escalada de privilégio - ver dados sensíveis de outra conta

**Objetivo**: como usuário comum, ler os dados sensíveis (inclusive a senha em texto plano) do administrador.

**Passos** (modo **Vulnerável**):
1. Garanta que está em `?IsSecureMode=false`.
2. Faça login como usuário comum: `john.doe@owasp-lab.local` / `password123`.
3. Observe no cabeçalho que seu papel é `User` (não `Admin`).
4. Na linha do **Administrator**, clique no botão 👁️ **Ver dados sensíveis**.

**Resultado esperado**:
```
📋 DADOS SENSÍVEIS DO USUÁRIO:
Nome: Administrator
Email: admin@owasp-lab.local
Senha: admin123 ⚠️ (texto plano!)
Role: Admin
⚠️ VULNERABILIDADE EXPLORADA - Usuário comum acessou dados sensíveis!
```

Um usuário sem papel administrativo leu a senha do admin. O correto seria um **403 Forbidden**.

> ✅ **Validado** contra o container: `john.doe` (Role User) recuperou a senha `admin123` do administrador no modo vulnerável.

### Nível 2: Escalada de privilégio - alterar o estado de outra conta

**Objetivo**: como usuário comum, desativar (ou reativar) a conta de outro usuário.

**Passos** (modo **Vulnerável**, ainda logado como `john.doe`):
1. Na linha de **Jane Smith**, clique no botão 🔀 **Ativar/Desativar** (ícone de toggle).

**Resultado esperado**:
```
Status alterado para: Inativo ⚠️ VULNERABILIDADE EXPLORADA!
```

Um usuário comum acabou de negar acesso a outra pessoa (uma pequena negação de serviço). Repita para reverter.

> ✅ **Validado**: `john.doe` alternou o status de Jane Smith no modo vulnerável.

### Nível 3: IDOR - trocando o identificador do alvo

**Objetivo**: entender que o alvo é um identificador controlado pelo cliente (`TargetUserId`) e que trocá-lo permite atingir qualquer conta, mesmo que a interface não oferecesse o botão.

No modo vulnerável, a página já renderiza os botões de ação para **todas** as outras linhas, então o "objeto alvo" está exposto diretamente. Para ver o mecanismo por trás, inspecione o formulário:

1. Abra as ferramentas de desenvolvedor do navegador (F12) na aba **Elements**.
2. Localize qualquer formulário de ação. Ele contém um campo oculto:
   ```html
   <input type="hidden" name="TargetUserId" value="3" />
   ```
3. Edite o `value` para `1` (o id do administrador) e envie o formulário pelo botão correspondente.

O servidor executa a operação sobre o id que você enviar, sem checar se aquele objeto é seu nem se você é admin. Esse é o padrão **IDOR**: a autorização depende de uma chave controlada pelo usuário (CWE-639).

**Prova por requisição direta** (opcional, para quem quer ver o HTTP): a ação é um `POST` para `...?IsSecureMode=false&handler=ViewSensitiveData` com os campos `TargetUserId`, `IsSecureMode` e o token antifalsificação `__RequestVerificationToken` (mais o cookie de sessão obtidos ao carregar a página). Enviando `TargetUserId=1` a partir de uma sessão de usuário comum, o servidor retorna os dados do administrador.

> ✅ **Validado**: uma requisição `ViewSensitiveData` com `TargetUserId=1` a partir da sessão de `john.doe` retornou os dados do administrador. Trocar o identificador basta para atingir outra conta.

### Nível 4: Impacto máximo - deletar uma conta

**Objetivo**: mostrar que a mesma falha permite uma operação destrutiva e irreversível.

**Passos** (modo **Vulnerável**, logado como `john.doe`):
1. Na linha de **Bob Wilson**, clique no botão 🗑️ **Deletar** e confirme.

**Resultado esperado**:
```
Usuário deletado com sucesso! ⚠️ VULNERABILIDADE EXPLORADA - Você não é admin mas conseguiu deletar!
```

O usuário some da tabela. Para restaurar o estado inicial do banco:
```bash
docker-compose down -v && docker-compose up -d
```

> ✅ **Validado**: `john.doe` (usuário comum) deletou Bob Wilson no modo vulnerável.

### Nível 5: Ligando o modo seguro

**Objetivo**: observar o mesmo ataque sendo recusado quando a autorização é validada.

**Passos**:
1. Clique em **🔒 Seguro** (a URL passa a `?IsSecureMode=true`). Faça login novamente, pois a sessão é renovada.
2. Logado como `john.doe` (usuário comum), tente qualquer ação (ver dados, desativar, deletar) sobre outra conta.

**Resultado esperado** (equivalente a um **403 Forbidden**):
```
❌ ACESSO NEGADO! Apenas administradores podem deletar usuários.
```

3. Faça logout e entre como administrador: `admin@owasp-lab.local` / `admin123`.
4. Repita a operação. Agora ela é autorizada:
```
✅ Usuário deletado com sucesso! Operação autorizada.
```

> ✅ **Validado**: no modo seguro, `john.doe` recebeu ACESSO NEGADO; o administrador executou a mesma operação com sucesso.

## 🛡️ Como Prevenir

### 1. Verifique autorização, não apenas autenticação

Estar logado (autenticado) não basta. Antes de qualquer ação sensível, cheque o papel/permissão:

```csharp
if (!user.IsAuthenticated) return Unauthorized();      // 401
if (user.Role != "Admin")  return Forbid();            // 403
```

### 2. Centralize o controle com RBAC declarativo

Em vez de repetir `if (role == "Admin")` em cada método, use o mecanismo do framework:

```csharp
[Authorize(Roles = "Admin")]
public IActionResult OnPostDeleteUser() { ... }
```

Assim a regra fica em um só lugar, é auditável e não pode ser esquecida em um endpoint novo.

### 3. Negue por padrão (deny by default)

O padrão deve ser recusar. Só libere o que foi explicitamente permitido. Endpoint sem política de autorização deveria ser rejeitado, não liberado.

### 4. Não confie em identificadores vindos do cliente (contra IDOR)

Valide a **posse** do objeto no servidor: o registro alvo pertence ao usuário logado, ou ele tem papel para agir sobre objetos de terceiros?

```csharp
if (target.OwnerId != currentUser.UserId && !currentUser.IsAdmin)
    return Forbid();
```

Considere também identificadores não sequenciais (GUID) para dificultar a enumeração, lembrando que isso é defesa em profundidade, não substitui a checagem de autorização.

### 5. Responda com o código HTTP correto

- **401** para não autenticado.
- **403** para autenticado sem permissão.

O código certo evita vazar informação e facilita o monitoramento (ver o lab A09).

### 6. Registre as tentativas negadas

Toda recusa de acesso é um sinal de segurança. O modo seguro deste lab já faz `LogWarning` em cada ACESSO NEGADO. Encadeie isso com o lab A09 (Security Logging Failures).

## 📊 Comparação

| Aspecto | Vulnerável | Seguro |
|---------|------------|--------|
| Checagem de papel | Nenhuma | `IsAdmin(currentUser)` antes da ação |
| Usuário comum deleta admin | ✅ Consegue | ❌ ACESSO NEGADO (403) |
| IDOR via `TargetUserId` | ✅ Explorável | ❌ Bloqueado pela autorização |
| Resposta a acesso indevido | Executa a operação | Recusa e registra em log |
| Risco | 🔴 Crítico | 🟢 Baixo |

## 🎓 Exercícios

### Exercício 1: Mapeie a falha no código
Em `VulnerableAccessControlService.cs`, identifique nos métodos `DeleteUser`, `ToggleUserStatus` e `ViewSensitiveData`:
1. Qual verificação está ausente.
2. Por que `WasAuthorized = true` é uma mentira nesse contexto.
3. O que aconteceria se a aplicação tivesse 50 endpoints e a checagem fosse feita à mão em cada um.

### Exercício 2: 401 x 403
Percorra os dois modos e responda:
1. Quando o comportamento correto seria **401** e quando seria **403**?
2. No modo vulnerável, qual código HTTP a operação deveria ter retornado e qual efeito ela teve na prática?

### Exercício 3: Compare implementações
Coloque `VulnerableAccessControlService.cs` e `SecureAccessControlService.cs` lado a lado e liste todas as diferenças. Qual é a única linha que realmente muda a postura de segurança?

### Exercício 4: IDOR na prática
Usando as ferramentas de desenvolvedor, altere o campo oculto `TargetUserId` e ataque uma conta diferente da que a interface sugeria. Depois, proponha em pseudocódigo a verificação de posse que impediria esse ataque.

### Exercício 5: Ponte para a Aula 14
Descreva, com o vocabulário da aula (autenticação, autorização, RBAC, 401, 403, deny by default), como você reescreveria o painel administrativo para que a falha fosse impossível por construção.

## 📚 Recursos Adicionais

- [OWASP A01:2021 - Broken Access Control](https://owasp.org/Top10/A01_2021-Broken_Access_Control/)
- [OWASP Authorization Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html)
- [CWE-284: Improper Access Control](https://cwe.mitre.org/data/definitions/284.html)
- [CWE-639: Authorization Bypass Through User-Controlled Key (IDOR)](https://cwe.mitre.org/data/definitions/639.html)
- [PortSwigger - Access control vulnerabilities](https://portswigger.net/web-security/access-control)

## ⚠️ Avisos Finais

1. **Ambiente controlado**: use este lab apenas em ambiente local isolado.
2. **Fins educacionais**: o conhecimento é para defesa, não ataque.
3. **Responsabilidade**: nunca teste controle de acesso em sistemas de terceiros sem autorização explícita.

---

**Lembre-se**: autenticação responde "quem é você"; autorização responde "o que você pode fazer". A A01 acontece quando a aplicação esquece a segunda pergunta. 🛡️
