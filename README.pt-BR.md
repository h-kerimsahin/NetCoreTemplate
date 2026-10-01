<div align="right">

[![Türkçe](https://img.shields.io/badge/Dil-Türkçe-red?style=for-the-badge)](README.md)
[![English](https://img.shields.io/badge/Language-English-blue?style=for-the-badge)](README.en.md)
[![Português (BR)](https://img.shields.io/badge/Idioma-Português%20(BR)-green?style=for-the-badge)](README.pt-BR.md)

</div>

# 🏗️ NetCoreTemplate — Template de API Clean Architecture .NET 11 Nível Produção

> Template moderno de ASP.NET Core Web API **pronto para produção**, multicamadas e focado em segurança.\
> Identity+RBAC, MediatR (CQRS), Hangfire, SignalR, OData v4, OpenTelemetry, Rate Limit de 3 camadas, Idempotência, Soft-Delete + Auditoria, Armazenamento de Arquivos Local/Azure, Swagger + Scalar UI.

***

## 📦 Baixar Template do Visual Studio (GitHub Release)

> Instale no Visual Studio com um clique e evite configuração do zero a cada novo projeto!

[![Latest Release](https://img.shields.io/github/v/release/h-kerimsahin/NetCoreTemplate?display_name=tag&sort=semver&label=Release&style=for-the-badge&logo=github)](https://github.com/h-kerimsahin/NetCoreTemplate/releases/latest)
[![Release Workflow Status](https://github.com/h-kerimsahin/NetCoreTemplate/actions/workflows/publish-template-release.yml/badge.svg)](https://github.com/h-kerimsahin/NetCoreTemplate/actions/workflows/publish-template-release.yml)

**Link:** [**Releases → Template ZIP**](https://github.com/h-kerimsahin/NetCoreTemplate/releases/latest)

| # | Passo |
|---|-------|
| 1 | Baixe **`KrmShnNetCoreBackendProject.zip`** da página de Releases |
| 2 | Copie para esta pasta: `%USERPROFILE%\Documents\Visual Studio 2022\Templates\ProjectTemplates\` |
| 3 | **Reinicie** o Visual Studio |
| 4 | *Create a new project* → pesquise: **`KrmShn`** → selecione **KrmShn Net Core Backend Project** |

**Publicando uma Release (Desenvolvedor):**
```powershell
# Opção A - release direta + upload com gh CLI
.\publish-release.ps1 -Version 1.2.0

# Opção B - apenas envie uma tag, publicação automática via workflow
git tag -a v1.2.0 -m "Release v1.2.0"
git push origin v1.2.0
```

***

## 🛡️ Pilha Tecnológica & Selos

| Camada            | Tecnologia                                                                                                          |
| ----------------- | ------------------------------------------------------------------------------------------------------------------- |
| **Runtime**       |  ASP.NET Core Web API (Minimal API)                                                                                 |
| **ORM**           |  SQL Server / LocalDB                                                                                               |
| **Padrão**        | **Clean Architecture 4 Camadas**, **CQRS** (MediatR 14 Single Package)                                              |
| **Autenticação**  | JWT (Access 15min / Refresh 7d), Modelo Customizado ASP.NET Identity, **2FA**, **RBAC** (Role+Permissão)            |
| **Jobs**          | Hangfire Dashboard + Jobs Recorrentes (Fallback de Armazenamento em Memória)                                        |
| **Tempo Real**    | SignalR Notification Hub                                                                                            |
| **Consulta**      | OData v4 (`$filter`, `$select`, `$orderby`, `$metadata`)                                                            |
| **Observabilidade**| OpenTelemetry 1.19.1 (Rastreio/Métrica/OTLP) + Prometheus `/metrics`                                                |
| **Log**           | Serilog Structured Log (Sinks Console/Arquivo/MSSQL)                                                                |
| **Documentação**  | Swagger UI + Scalar UI (OpenAPI versionado v1 & v2)                                                                 |
| **Segurança**     | Rate Limit de 3 Camadas, Filtro X-Idempotency-Key, Proteção contra Path Traversal, CORS Seguro por Padrão, Seed Produção Desligado |
| **Validação**     | FluentValidation + Mapeamento Mapster                                                                               |

***

## 📁 Estrutura do Projeto (Clean Architecture — 4 Camadas)

```
NetCoreTemplate/
├── Core/
│   ├── NetCoreTemplate.Domain/            # Entidade, Enum, IRepository, Value Object
│   ├── NetCoreTemplate.Application/       # DTO, CQRS Command/Query, Validador, Mapeamentos
│   └── NetCoreTemplate.Infrastructure/    # EF Core, UoW, Hangfire, Serilog, Armazenamento, E-mail, OTel
└── Presentation/
    └── NetCoreTemplate.Api/               # Minimal API Endpoints (11 ao todo), SignalR Hub, UI, Program
```

| Camada             | Conteúdo Exemplo                                                                                    |
| ------------------ | --------------------------------------------------------------------------------------------------- |
| **Domain**         | `AppUser`, `AppRole`, `AppPermission`, `EntityStatus`, `IRepository<T>`                             |
| **Application**    | `LoginCommand`, `RegisterCommand`, `MeQuery`, `Validador FluentValidation`                          |
| **Infrastructure** | `AppDbContext`, `UnitOfWork`, `LocalFileStorageService`, `HangfireJobScheduler`, `EmailSender`      |
| **API**            | `AuthEndpoints.cs`, `RolesEndpoints.cs`, `NotificationHub.cs`, `Program.cs`                         |

***

## 👤 Seed Padrão (SOMENTE Ambiente de Desenvolvimento)

> ⚠️ **Este usuário NÃO é criado em Produção** — AppDbInitializer verifica com `IHostEnvironment.IsDevelopment()`.

| Informação           | Valor                                          |
| -------------------- | ---------------------------------------------- |
| Nome de Usuário      | `SuperAdmin`                                   |
| E-mail               | `superadmin@netcoretemplate.com`               |
| Senha                | `Qwerty123!`                                   |
| Função               | `SuperAdmin`                                   |
| Funções com Seed Automático | `SuperAdmin`, `Admin`, `User`             |
| Permissões           | Todas as permissões aplicáveis são semeadas na tabela |

> 📌 **Para Produção**: Crie o primeiro SuperAdmin manualmente via script DB ou CLI.

***

## 🚀 Início Rápido (Execute em 5 Passos)

```bash
# 1. Restaurar
dotnet restore

# 2. Criar DB (Migration)
cd Presentation/NetCoreTemplate.Api
dotnet ef database update

# 3. Executar
dotnet run
```

| Endereço                  | O que                                                   |
| ------------------------- | ------------------------------------------------------- |
| 🌐 HTTP                   | `http://localhost:5084`                                 |
| 🔐 HTTPS                  | `https://localhost:7084`                                |
| 📘 **Swagger UI**         | `http://localhost:5084/swagger`                         |
| 🎨 **Scalar UI (Moderno)**| `http://localhost:5084/scalar` & `/scalar/v2`           |
| ⏱️ Hangfire Dashboard     | `http://localhost:5084/hangfire` (SuperAdmin)           |
| 🏥 Health Readiness       | `http://localhost:5084/api/v1/health/ready`             |
| 📊 Métricas Prometheus    | `http://localhost:5084/metrics`                         |
| 🧩 OData \$metadata       | `/api/v1/odata/$metadata` & `/api/v2/odata/$metadata`   |

***

## 🔌 Todos os Endpoints (35+ REST + 8 UI/Painel)

Todas as APIs retornam **resposta padrão encapsulada** (`ApiResponse<T>`):

```json
{
  "statusCode": 200,
  "message": "Operação bem-sucedida.",
  "data": { "...": "..." },
  "errors": null,
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 123
}
```

> 📌 Endpoints paginados: `?page=1&size=20` (padrão)\
> 📌 Idempotência: No POST/PUT use o header `X-Idempotency-Key: <guid>` → você não enviará a mesma operação duas vezes.

***

### 🔐 GRUPO A — Autenticação (12 Endpoints)

**Prefixo:** `api/v1/auth`

| #   | Método | Rota                  | Auth              | Idem | Descrição                                            |
| --- | ------ | --------------------- | ----------------- | ---- | ---------------------------------------------------- |
| A1  | `POST` | `/login`              | 🔓 AllowAnonymous | ✅    | Login do usuário (E-mail/NomeUsuário + Senha)        |
| A2  | `POST` | `/register`           | 🔓 AllowAnonymous | ✅    | Registro de novo usuário                             |
| A3  | `POST` | `/refresh`            | 🔓 AllowAnonymous | ✅    | Atualizar Access Token (Refresh Token)               |
| A4  | `POST` | `/change-password`    | 🔑 Bearer         | ✅    | Usuário logado altera sua própria senha              |
| A5  | `POST` | `/confirm-email`      | 🔓 AllowAnonymous | ✅    | Confirmar e-mail                                     |
| A6  | `POST` | `/forgot-password`    | 🔓 AllowAnonymous | ✅    | Solicitar token de redefinição de senha              |
| A7  | `POST` | `/reset-password`     | 🔓 AllowAnonymous | ✅    | Definir nova senha com token                         |
| A8  | `POST` | `/enable-2fa`         | 🔑 Bearer         | ✅    | Habilitar 2FA                                        |
| A9  | `POST` | `/verify-2fa`         | 🔓 AllowAnonymous | ✅    | Verificar Código 2FA / Login                         |
| A10 | `POST` | `/logout`             | 🔑 Bearer         | ✅    | Logout de único dispositivo (Revoga Refresh Token)   |
| A11 | `POST` | `/logout-all-devices` | 🔑 Bearer         | ✅    | Logout de **TODOS** os dispositivos (Atualiza SecurityStamp)|
| A12 | `GET`  | `/me`                 | 🔑 Bearer         | ❌    | Informações do usuário atual + Funções + Permissões  |

#### Exemplo — Requisição de Login (A1)

```json
{
  "emailOrUserName": "SuperAdmin",
  "password": "Qwerty123!",
  "rememberMe": true
}
```

#### Exemplo — Resposta de Login (200 OK)

```json
{
  "statusCode": 200,
  "message": "Login bem-sucedido.",
  "data": {
    "accessToken": "<JWT ACCESS TOKEN — 15min>",
    "accessTokenExpiresAt": "2026-10-01T11:30:00Z",
    "refreshToken": "<REFRESH TOKEN — 7 dias>",
    "refreshTokenExpiresAt": "2026-10-08T11:15:00Z",
    "user": {
      "id": "00000000-0000-0000-0000-000000000001",
      "userName": "SuperAdmin",
      "email": "superadmin@netcoretemplate.com",
      "roles": ["SuperAdmin"],
      "permissions": ["*.*"]
    }
  }
}
```

***

### 🛠️ GRUPO B — Gerenciamento de Usuários (5 Endpoints)

**Prefixo:** `api/v1/users-management`\
**Função Requerida:** `Admin` **OU** `SuperAdmin` (Todo o grupo)

| #  | Método | Rota                           | Idem | Descrição                                                                 |
| -- | ------ | ------------------------------ | ---- | ------------------------------------------------------------------------- |
| B1 | `POST` | `/assign-role`                 | ✅    | **Atribuir novas funções** ao usuário (funções antigas excluídas, sobrescritas)|
| B2 | `POST` | `/revoke-role`                 | ✅    | **Revogar funções específicas** do usuário                                |
| B3 | `POST` | `/{userId}/logout-all-devices` | ✅    | Admin → Deslogar **qualquer usuário** de todos os dispositivos            |
| B4 | `POST` | `/{userId}/unlock`             | ✅    | Desbloquear usuário (bloqueado após 5 tentativas falhas)                  |
| B5 | `GET`  | `/paged?page=1&size=20`        | ❌    | Listar todos os usuários paginados                                        |

#### Exemplo — AssignRole (B1) Requisição

```json
{
  "userId": "a1b2c3d4-...",
  "roleIds": [ "role1-guid", "role2-guid" ]
}
```

***

### 🏷️ GRUPO C — Funções & Permissões (6 Endpoints)

**Prefixo:** `api/v1/roles`\
**Função Requerida:** `Admin` **OU** `SuperAdmin`

| #  | Método   | Rota                    | Idem | Descrição                                                    |
| -- | -------- | ----------------------- | ---- | ------------------------------------------------------------ |
| C1 | `POST`   | `/`                     | ✅    | Criar nova função (baseado em soft-delete)                   |
| C2 | `PUT`    | `/{id:guid}`            | ✅    | Atualizar nome & descrição da função                         |
| C3 | `DELETE` | `/{id:guid}`            | ❌    | Excluir função (**Soft-Delete**, Status=Deleted)             |
| C4 | `POST`   | `/{roleId}/permissions` | ✅    | **Atribuir permissões** à função (excluir existentes, gravar nova lista) |
| C5 | `GET`    | `/{id:guid}`            | ❌    | Obter detalhes da função + permissões                        |
| C6 | `GET`    | `/?page=1&size=20`      | ❌    | Listar todas as funções paginadas (suporta OData)            |

#### Exemplo — AssignPermission (C4)

```json
{
  "roleId": "<GUID DA FUNÇÃO>",
  "permissionIds": [ "perm-view-users", "perm-manage-products" ]
}
```

***

### 👤 GRUPO D — Perfis de Usuário (3 Endpoints)

**Prefixo:** `api/v1/user-profiles`\
**Auth:** 🔑 Bearer JWT (Qualquer pessoa logada)

| #  | Método | Rota               | Idem | Descrição                                                  |
| -- | ------ | ------------------ | ---- | ---------------------------------------------------------- |
| D1 | `GET`  | `/{id:guid}`       | ❌    | Obter informações do perfil do usuário por ID              |
| D2 | `GET`  | `/?page=1&size=20` | ❌    | Listar todos os perfis paginados                           |
| D3 | `PUT`  | `/mine`            | ✅    | Atualizar **MEU PRÓPRIO** perfil (userId extraído do JWT)  |

#### Exemplo — UpdateMyProfile (D3)

```json
{
  "firstName": "João",
  "lastName": "Silva",
  "birthDate": "1992-03-15T00:00:00Z",
  "phoneNumber": "+5511999999999",
  "address": "Rua 1",
  "city": "São Paulo",
  "country": "Brasil",
  "avatarUrl": "/uploads/avatars/2026/10/01/abc123.png",
  "bio": "Desenvolvedor de Software"
}
```

***

### 💾 GRUPO E — Armazenamento de Arquivos / Upload (2 Endpoints)

**Prefixo:** `api/v1/file-storage`\
**Auth:** 🔑 Bearer JWT

> 🚧 **Contêineres Permitidos (Lista Branca — senão 400):** `avatars` · `documents` · `temp` · `exports`

| #  | Método   | Rota                                  | Idem | Descrição                                                 |
| -- | -------- | ------------------------------------- | ---- | --------------------------------------------------------- |
| E1 | `POST`   | `/upload`                             | ✅    | Upload de arquivo multipart form (JWT + Antiforgery desabilitado)|
| E2 | `DELETE` | `/delete?filePath=/uploads/.../x.png` | ❌    | Excluir arquivo via URL relativa (Protegido contra Path Traversal) |

#### Exemplo — Upload (E1)

**Content-Type:** `multipart/form-data`

| Campo Formulário | Tipo       | Descrição                                                        |
| ---------------- | ---------- | ---------------------------------------------------------------- |
| `file`           | IFormFile  | **Obrigatório**, arquivo a ser carregado                         |
| `container`      | string     | `avatars` / `documents` / `temp` / `exports`                     |

**Resposta do Upload (201 Created):**

```json
{
  "statusCode": 201,
  "message": "Arquivo carregado.",
  "data": {
    "url": "/uploads/avatars/2026/10/01/a1b2c3d4.png",
    "fileName": "<guid>_perfil.png",
    "originalName": "perfil.png",
    "size": 2048576
  }
}
```

> 🛡️ **Segurança:** `DeleteFileAsync` + `FileExistsAsync` → normalização `Path.GetFullPath()` + verificação `StartsWith(WebRootPath)` → **100% protegido contra ataques de Path Traversal**.

***

### 🔔 GRUPO F — Notificações + SignalR Hub (4+1)

**Prefixo:** `api/v1/notifications` + **Hub:** `/hubs/notification`

| #  | Método | Rota                   | Função              | Idem | Descrição                                          |
| -- | ------ | ---------------------- | ------------------- | ---- | -------------------------------------------------- |
| F1 | `POST` | `/send`                | `Admin\|SuperAdmin` | ✅    | Enviar notificação (DB + **Push em Tempo Real SignalR**)|
| F2 | `GET`  | `/mine?page=1&size=20` | 🔑 Todos            | ❌    | Minhas notificações (Data DESC)                    |
| F3 | `POST` | `/{id}/mark-as-read`   | 🔑 Todos            | ✅    | Marcar notificação única como lida                 |
| F4 | `POST` | `/mark-all-as-read`    | 🔑 Todos            | ✅    | Marcar **TODAS** as minhas notificações como lidas |

#### Exemplo — Enviar Notificação (F1)

```json
{
  "userId": "<GUID DO USUÁRIO ou NULL (Todos)>",
  "title": "Novo Pedido",
  "message": "Seu pedido #1234 está a caminho.",
  "type": 1,
  "data": "{\"orderId\": 1234,\"link\":\"/orders/1234\"}"
}
```

> `type`: `0=Info, 1=Sucesso, 2=Aviso, 3=Erro`

#### 🛰️ SignalR Hub (Exemplo de Cliente JS)

```javascript
const hub = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7084/hubs/notification?access_token=" + JWT_TOKEN)
  .build();

hub.on("ReceiveNotification", (notif) => console.log("Notificação!", notif));
await hub.start(); // Quando a conexão é estabelecida o usuário entra automaticamente em seu próprio grupo
```

***

### 🧾 GRUPO G — Entradas de Auditoria / Histórico de Alterações (2)

**Prefixo:** `api/v1/audit-entries`\
**Função:** `Admin \| SuperAdmin`

| #  | Método | Rota                                             | Descrição                                                             |
| -- | ------ | ------------------------------------------------ | --------------------------------------------------------------------- |
| G1 | `GET`  | `/entity/{entityName}/{entityId}?page=1&size=20` | Histórico de alterações de **entidade única** (ex.: `AppUser`, `AppRole`)|
| G2 | `GET`  | `/user/{changedByUserId}?page=1&size=20`         | **TODAS as** alterações feitas por **um usuário**                     |

Cada registro: `ChangeType = Added / Modified / Deleted / Restored` + `OldValues` + `NewValues` (JSON diff).

***

### 🔄 GRUPO H — Restaurar Soft Delete (2)

**Prefixo:** `api/v1/soft-restore`\
**Função:** `Admin \| SuperAdmin`

| #  | Método | Rota                                       | Idem | Descrição                                                              |
| -- | ------ | ------------------------------------------ | ---- | ---------------------------------------------------------------------- |
| H1 | `GET`  | `/deleted/{entityTypeName}?page=1&size=20` | ❌    | Listar registros excluídos (`appuser\|user`, `approle\|role`)           |
| H2 | `POST` | `/restore`                                 | ✅    | Restaurar registro excluído (`Status: Deleted → Active`)               |

```json
// Requisição Restore (H2)
{
  "entityTypeName": "appuser",
  "entityId": "<GUID do registro excluído>"
}
```

***

### 📝 GRUPO I — Logs do Sistema & Atividades do Usuário (2)

**Prefixo:** `api/v1/system-logs` · **Auth:** 🔑 Bearer JWT

| #  | Método | Rota                                    | Descrição                                                                                                     |
| -- | ------ | --------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| I1 | `GET`  | `/?level=Error&page=1&size=20`          | Logs do sistema (Serilog → DB). `level`: Information/Warning/Error/Fatal                                     |
| I2 | `GET`  | `/activities?type=Login&page=1&size=20` | Atividades do usuário. `type`: Login/Logout/FailedLogin/PasswordChange/ProfileUpdate/RoleAssigned...          |

***

### 🏥 GRUPO J — Saúde / Monitoramento (4+2)

**Prefixo:** `api/v1/health` · **Auth:** 🔓 AllowAnonymous

| #  | Método | Rota (Endpoint)  | Descrição                                                  | Alternativa Cloud MW                 |
| -- | ------ | ---------------- | ---------------------------------------------------------- | ------------------------------------ |
| J1 | `GET`  | `/live`          | Liveness — apenas o pipeline HTTP está funcionando?        | `/health/live` (MapHealthChecks)     |
| J2 | `GET`  | `/ready`         | Readiness — verificar conexões **DB + SMTP + Hangfire**    | `/health/ready` (Custom JSON Writer) |
| J3 | `GET`  | `/detailed`      | Detalhado — todos os registros de Saúde + Exceção/Dados    | -                                    |
| J4 | `GET`  | `/metrics`       | Info: Endpoint Prometheus `/metrics`                       | -                                    |

#### Endpoint Real de Scraping do Prometheus

***

### 🧪 GRUPO K — Teste da API V2 (1 Endpoint)

**Prefixo:** `api/v2/test`

| Método | Rota     | Descrição                                                         |
| ------ | -------- | ----------------------------------------------------------------- |
| `GET`  | `/dummy` | Teste de funcionamento do Versionamento de API v2 (Retorna v2.0 JSON)|

***

### 🎨 GRUPO L — Painéis de UI

**Somente** **`env.IsDevelopment()`** → desabilitado em produção (exceto Hangfire).

| URL                        | O que                                                                                | Acesso                                                               |
| -------------------------- | ------------------------------------------------------------------------------------ | -------------------------------------------------------------------- |
| `/swagger`                 | Swagger UI v1 + v2 (OpenAPI)                                                         | Desenvolvimento (Botão Authorize + Bearer para auth)                |
| `/swagger/v1/swagger.json` | v1 OpenAPI JSON                                                                      | Desenvolvimento                                                      |
| `/swagger/v2/swagger.json` | v2 OpenAPI JSON                                                                      | Desenvolvimento                                                      |
| `/scalar`                  | 🎨 Scalar UI — Documentação de API de aparência moderna (v1)                         | Desenvolvimento                                                      |
| `/scalar/v2`               | Scalar UI (v2)                                                                       | Desenvolvimento                                                      |
| **`/hangfire`**            | ⏱️ Painel Hangfire: Fila, Job, Retentativas, Recorrentes (nightly-cleanup 02:00 UTC) | 🔐 **Função SuperAdmin** (Todos os ambientes, filtro de auth custom) |
| `/api/v1/odata/$metadata`  | OData v4 EDM Model v1                                                                | AllowAnonymous                                                       |
| `/api/v2/odata/$metadata`  | OData v4 EDM Model v2                                                                | AllowAnonymous                                                       |

***

## 🛡️ Camadas de Segurança (Middleware & Filtro)

| Camada                        | Como Funciona                                                                                                                            |
| ----------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| **SecurityStampMiddleware**   | Em cada requisição compara `JWT SecurityStamp` ↔ `DB SecurityStamp`; se não coincidir JWT é inválido (para LogoutAllDevices)              |
| **IdempotencyEndpointFilter** | No POST/PUT usa `X-Idempotency-Key` → se a mesma chave chegar novamente retorna resultado em cache do DB                                 |
| **Rate Limit de 3 Camadas**   | `AuthFixedWindow` (10/10min), `GlobalSliding` (100/1min), `IPConcurrency` (200/30min)                                                    |
| **CORS Seguro por Padrão**    | `AllowCredentials = false` + `AllowedOrigins` vazio → ser preenchido para Admin/Frontend                                                  |
| **Upload Path Traversal**     | Normalização `Path.GetFullPath` + `StartsWith(WebRootPath)` → arquivos fora da raiz bloqueados                                            |
| **Seed Produção Desligado**   | Seed SuperAdmin roda **somente em Desenvolvimento** → nenhuma senha conhecida em Produção                                                 |
| **Lista Branca de Upload**    | Contêineres fora de `avatars / documents / temp / exports` → 400                                                                          |

***

## 🏭 Checklist de Implantação em Produção (OBRIGATÓRIO)

***

## 📌 Resumo Total

| Categoria                                               | Quantidade                 |
| ------------------------------------------------------- | -------------------------- |
| 🔐 Endpoint de Autenticação                             | **12**                     |
| 🛠️ Gerenciamento de Usuários                            | **5**                      |
| 🏷️ Funções + Permissões                                 | **6**                      |
| 👤 Perfis de Usuário                                    | **3**                      |
| 💾 Armazenamento de Arquivos                            | **2**                      |
| 🔔 Notificações + SignalR Hub                           | **4 + 1**                  |
| 🧾 Entradas de Auditoria                                | **2**                      |
| 🔄 Restaurar Soft Delete                                | **2**                      |
| 📝 Logs do Sistema + Atividades                         | **2**                      |
| 🏥 Saúde + Monitoramento                                | **4 + 2 Caminhos de Middleware** |
| 🧪 V2 Dummy                                             | **1**                      |
| 🎨 Painel de UI (Swagger/Scalar/Hangfire/OData/Métricas)| **8**                      |
| **TOTAL**                                               | **43+ Endpoints**          |

***

## 📄 Licença & Documentos
