<div align="right">

[![Türkçe](https://img.shields.io/badge/Dil-Türkçe-red?style=for-the-badge)](README.md)
[![English](https://img.shields.io/badge/Language-English-blue?style=for-the-badge)](README.en.md)
[![Português (BR)](https://img.shields.io/badge/Idioma-Português%20(BR)-green?style=for-the-badge)](README.pt-BR.md)

</div>

# 🏗️ NetCoreTemplate — Production Grade .NET 11 Clean Architecture API

> **Production-ready**, multi-layered, security-focused modern ASP.NET Core Web API template.\
> Identity+RBAC, MediatR (CQRS), Hangfire, SignalR, OData v4, OpenTelemetry, 3-layer Rate Limit, Idempotency, Soft-Delete + Audit, Local/Azure File Storage, Swagger + Scalar UI.

***

## 📦 Download Visual Studio Template (GitHub Release)

> Install to Visual Studio with one click and skip zero-setup for every new project!

[![Latest Release](https://img.shields.io/github/v/release/h-kerimsahin/NetCoreTemplate?display_name=tag&sort=semver&label=Release&style=for-the-badge&logo=github)](https://github.com/h-kerimsahin/NetCoreTemplate/releases/latest)
[![Release Workflow Status](https://github.com/h-kerimsahin/NetCoreTemplate/actions/workflows/publish-template-release.yml/badge.svg)](https://github.com/h-kerimsahin/NetCoreTemplate/actions/workflows/publish-template-release.yml)

**Link:** [**Releases → Template ZIP**](https://github.com/h-kerimsahin/NetCoreTemplate/releases/latest)

| # | Step |
|---|------|
| 1 | Download **`KrmShnNetCoreBackendProject.zip`** from the Releases page |
| 2 | Copy to this folder: `%USERPROFILE%\Documents\Visual Studio 2022\Templates\ProjectTemplates\` |
| 3 | **Restart** Visual Studio |
| 4 | *Create a new project* → search: **`KrmShn`** → **KrmShn Net Core Backend Project** |

**Publishing a Release (Developer):**
```powershell
# Option A - direct release + upload with gh CLI
.\publish-release.ps1 -Version 1.2.0

# Option B - just push a tag, auto-publish via workflow
git tag -a v1.2.0 -m "Release v1.2.0"
git push origin v1.2.0
```

***

## 🛡️ Tech Stack & Badges

| Layer             | Technology                                                                                                          |
| ----------------- | ------------------------------------------------------------------------------------------------------------------- |
| **Runtime**       |  ASP.NET Core Web API (Minimal API)                                                                                 |
| **ORM**           |  SQL Server / LocalDB                                                                                               |
| **Pattern**       | **Clean Architecture 4 Layers**, **CQRS** (MediatR 14 Single Package)                                               |
| **Auth**          | JWT (Access 15min / Refresh 7d), ASP.NET Identity Custom Model, **2FA**, **RBAC** (Role+Permission)                 |
| **Jobs**          | Hangfire Dashboard + Recurring Jobs (Memory Storage Fallback)                                                       |
| **Realtime**      | SignalR Notification Hub                                                                                            |
| **Query**         | OData v4 (`$filter`, `$select`, `$orderby`, `$metadata`)                                                            |
| **Observability** | OpenTelemetry 1.19.1 (Trace/Metric/OTLP) + Prometheus `/metrics`                                                    |
| **Logging**       | Serilog Structured Log (Console/File/MSSQL sinks)                                                                   |
| **Docs**          | Swagger UI + Scalar UI (v1 & v2 versioned OpenAPI)                                                                  |
| **Security**      | 3-Layer Rate Limit, X-Idempotency-Key Filter, Path Traversal Protection, CORS Secure-by-Default, Production Seed Off |
| **Validation**    | FluentValidation + Mapster Mapping                                                                                  |

***

## 📁 Project Structure (Clean Architecture — 4 Layers)

```
NetCoreTemplate/
├── Core/
│   ├── NetCoreTemplate.Domain/            # Entity, Enum, IRepository, Value Object
│   ├── NetCoreTemplate.Application/       # DTO, CQRS Command/Query, Validator, Mappings
│   └── NetCoreTemplate.Infrastructure/    # EF Core, UoW, Hangfire, Serilog, Storage, Email, OTel
└── Presentation/
    └── NetCoreTemplate.Api/               # Minimal API Endpoints (11 total), SignalR Hub, UI, Program
```

| Layer              | Example Content                                                                                   |
| ------------------ | ------------------------------------------------------------------------------------------------- |
| **Domain**         | `AppUser`, `AppRole`, `AppPermission`, `EntityStatus`, `IRepository<T>`                           |
| **Application**    | `LoginCommand`, `RegisterCommand`, `MeQuery`, `FluentValidation Validator`                        |
| **Infrastructure** | `AppDbContext`, `UnitOfWork`, `LocalFileStorageService`, `HangfireJobScheduler`, `EmailSender`    |
| **API**            | `AuthEndpoints.cs`, `RolesEndpoints.cs`, `NotificationHub.cs`, `Program.cs`                       |

***

## 👤 Default Seed (DEVELOPMENT Environment ONLY)

> ⚠️ **This user is NOT created in Production** — AppDbInitializer checks with `IHostEnvironment.IsDevelopment()`.

| Info                 | Value                                          |
| -------------------- | ---------------------------------------------- |
| Username             | `SuperAdmin`                                   |
| Email                | `superadmin@netcoretemplate.com`               |
| Password             | `Qwerty123!`                                   |
| Role                 | `SuperAdmin`                                   |
| Auto-Seeded Roles    | `SuperAdmin`, `Admin`, `User`                  |
| Permissions          | All applicable permissions are seeded to the table |

> 📌 **For Production**: Create the first SuperAdmin manually via DB script or CLI.

***

## 🚀 Quick Start (Run in 5 Steps)

```bash
# 1. Restore
dotnet restore

# 2. Create DB (Migration)
cd Presentation/NetCoreTemplate.Api
dotnet ef database update

# 3. Run
dotnet run
```

| Address                   | What                                                    |
| ------------------------- | ------------------------------------------------------- |
| 🌐 HTTP                   | `http://localhost:5084`                                 |
| 🔐 HTTPS                  | `https://localhost:7084`                                |
| 📘 **Swagger UI**         | `http://localhost:5084/swagger`                         |
| 🎨 **Scalar UI (Modern)** | `http://localhost:5084/scalar` & `/scalar/v2`           |
| ⏱️ Hangfire Dashboard     | `http://localhost:5084/hangfire` (SuperAdmin)           |
| 🏥 Health Readiness       | `http://localhost:5084/api/v1/health/ready`             |
| 📊 Prometheus Metrics     | `http://localhost:5084/metrics`                         |
| 🧩 OData \$metadata       | `/api/v1/odata/$metadata` & `/api/v2/odata/$metadata`   |

***

## 🔌 All Endpoints (35+ REST + 8 UI/Dashboard)

All APIs return **standard wrapped response** (`ApiResponse<T>`):

```json
{
  "statusCode": 200,
  "message": "Operation successful.",
  "data": { "...": "..." },
  "errors": null,
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 123
}
```

> 📌 Paged endpoints: `?page=1&size=20` (default)\
> 📌 Idempotency: On POST/PUT use `X-Idempotency-Key: <guid>` header → you won't send the same operation twice.

***

### 🔐 GROUP A — Authentication (12 Endpoints)

**Prefix:** `api/v1/auth`

| #   | Method | Route                 | Auth              | Idem | Description                                        |
| --- | ------ | --------------------- | ----------------- | ---- | -------------------------------------------------- |
| A1  | `POST` | `/login`              | 🔓 AllowAnonymous | ✅    | User login (Email/Username + Password)             |
| A2  | `POST` | `/register`           | 🔓 AllowAnonymous | ✅    | New user registration                              |
| A3  | `POST` | `/refresh`            | 🔓 AllowAnonymous | ✅    | Refresh Access Token (Refresh Token)               |
| A4  | `POST` | `/change-password`    | 🔑 Bearer         | ✅    | Logged-in user changes their own password          |
| A5  | `POST` | `/confirm-email`      | 🔓 AllowAnonymous | ✅    | Confirm email                                      |
| A6  | `POST` | `/forgot-password`    | 🔓 AllowAnonymous | ✅    | Request password reset token                       |
| A7  | `POST` | `/reset-password`     | 🔓 AllowAnonymous | ✅    | Set new password with token                        |
| A8  | `POST` | `/enable-2fa`         | 🔑 Bearer         | ✅    | Enable 2FA                                         |
| A9  | `POST` | `/verify-2fa`         | 🔓 AllowAnonymous | ✅    | Verify 2FA Code / Login                            |
| A10 | `POST` | `/logout`             | 🔑 Bearer         | ✅    | Logout from single device (Refresh Token Revoke)   |
| A11 | `POST` | `/logout-all-devices` | 🔑 Bearer         | ✅    | Logout from **ALL** devices (SecurityStamp Refresh)|
| A12 | `GET`  | `/me`                 | 🔑 Bearer         | ❌    | Current user info + Roles + Permissions            |

#### Example — Login Request (A1)

```json
{
  "emailOrUserName": "SuperAdmin",
  "password": "Qwerty123!",
  "rememberMe": true
}
```

#### Example — Login Response (200 OK)

```json
{
  "statusCode": 200,
  "message": "Login successful.",
  "data": {
    "accessToken": "<JWT ACCESS TOKEN — 15min>",
    "accessTokenExpiresAt": "2026-10-01T11:30:00Z",
    "refreshToken": "<REFRESH TOKEN — 7 days>",
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

### 🛠️ GROUP B — User Management (5 Endpoints)

**Prefix:** `api/v1/users-management`\
**Required Role:** `Admin` **OR** `SuperAdmin` (Entire group)

| #  | Method | Route                          | Idem | Description                                                              |
| -- | ------ | ------------------------------ | ---- | ------------------------------------------------------------------------ |
| B1 | `POST` | `/assign-role`                 | ✅    | **Assign new roles** to user (old roles deleted, overwritten)            |
| B2 | `POST` | `/revoke-role`                 | ✅    | **Revoke specific roles** from user                                      |
| B3 | `POST` | `/{userId}/logout-all-devices` | ✅    | Admin → Log **any user** out from all devices                            |
| B4 | `POST` | `/{userId}/unlock`             | ✅    | Unlock user (locked after 5 failed attempts)                             |
| B5 | `GET`  | `/paged?page=1&size=20`        | ❌    | List all users paged                                                     |

#### Example — AssignRole (B1) Request

```json
{
  "userId": "a1b2c3d4-...",
  "roleIds": [ "role1-guid", "role2-guid" ]
}
```

***

### 🏷️ GROUP C — Roles & Permissions (6 Endpoints)

**Prefix:** `api/v1/roles`\
**Required Role:** `Admin` **OR** `SuperAdmin`

| #  | Method   | Route                   | Idem | Description                                                  |
| -- | -------- | ----------------------- | ---- | ------------------------------------------------------------ |
| C1 | `POST`   | `/`                     | ✅    | Create new role (soft-delete based)                          |
| C2 | `PUT`    | `/{id:guid}`            | ✅    | Update role name & description                               |
| C3 | `DELETE` | `/{id:guid}`            | ❌    | Delete role (**Soft-Delete**, Status=Deleted)                |
| C4 | `POST`   | `/{roleId}/permissions` | ✅    | **Assign permissions** to role (delete existing, write new)  |
| C5 | `GET`    | `/{id:guid}`            | ❌    | Get role + permissions details                               |
| C6 | `GET`    | `/?page=1&size=20`      | ❌    | List all roles paged (OData supported)                       |

#### Example — AssignPermission (C4)

```json
{
  "roleId": "<ROLE GUID>",
  "permissionIds": [ "perm-view-users", "perm-manage-products" ]
}
```

***

### 👤 GROUP D — User Profiles (3 Endpoints)

**Prefix:** `api/v1/user-profiles`\
**Auth:** 🔑 Bearer JWT (Anyone logged in)

| #  | Method | Route              | Idem | Description                                            |
| -- | ------ | ------------------ | ---- | ------------------------------------------------------ |
| D1 | `GET`  | `/{id:guid}`       | ❌    | Get user profile info by ID                            |
| D2 | `GET`  | `/?page=1&size=20` | ❌    | List all profiles paged                                |
| D3 | `PUT`  | `/mine`            | ✅    | Update **MY OWN** profile (userId pulled from JWT)     |

#### Example — UpdateMyProfile (D3)

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "birthDate": "1992-03-15T00:00:00Z",
  "phoneNumber": "+15551234567",
  "address": "Street 1",
  "city": "Istanbul",
  "country": "Turkey",
  "avatarUrl": "/uploads/avatars/2026/10/01/abc123.png",
  "bio": "Software Developer"
}
```

***

### 💾 GROUP E — File Storage / Upload (2 Endpoints)

**Prefix:** `api/v1/file-storage`\
**Auth:** 🔑 Bearer JWT

> 🚧 **Allowed Containers (Whitelist — otherwise 400):** `avatars` · `documents` · `temp` · `exports`

| #  | Method   | Route                                 | Idem | Description                                              |
| -- | -------- | ------------------------------------- | ---- | -------------------------------------------------------- |
| E1 | `POST`   | `/upload`                             | ✅    | Upload multipart form file (JWT + Antiforgery disabled)  |
| E2 | `DELETE` | `/delete?filePath=/uploads/.../x.png` | ❌    | Delete file via relative URL (Path Traversal Protected)  |

#### Example — Upload (E1)

**Content-Type:** `multipart/form-data`

| Form Field  | Type       | Description                                              |
| ----------- | ---------- | -------------------------------------------------------- |
| `file`      | IFormFile  | **Required**, file to upload                             |
| `container` | string     | `avatars` / `documents` / `temp` / `exports`             |

**Upload Response (201 Created):**

```json
{
  "statusCode": 201,
  "message": "File uploaded.",
  "data": {
    "url": "/uploads/avatars/2026/10/01/a1b2c3d4.png",
    "fileName": "<guid>_profile.png",
    "originalName": "profile.png",
    "size": 2048576
  }
}
```

> 🛡️ **Security:** `DeleteFileAsync` + `FileExistsAsync` → `Path.GetFullPath()` normalize + `StartsWith(WebRootPath)` check → **100% protected against Path Traversal attacks**.

***

### 🔔 GROUP F — Notifications + SignalR Hub (4+1)

**Prefix:** `api/v1/notifications` + **Hub:** `/hubs/notification`

| #  | Method | Route                  | Role                | Idem | Description                                         |
| -- | ------ | ---------------------- | ------------------- | ---- | --------------------------------------------------- |
| F1 | `POST` | `/send`                | `Admin\|SuperAdmin` | ✅    | Send notification (DB + **SignalR Realtime Push**)  |
| F2 | `GET`  | `/mine?page=1&size=20` | 🔑 Everyone         | ❌    | My notifications (Date DESC)                        |
| F3 | `POST` | `/{id}/mark-as-read`   | 🔑 Everyone         | ✅    | Mark single notification as read                    |
| F4 | `POST` | `/mark-all-as-read`    | 🔑 Everyone         | ✅    | Mark **ALL** my notifications as read               |

#### Example — Send Notification (F1)

```json
{
  "userId": "<USER GUID or NULL (Everyone)>",
  "title": "New Order",
  "message": "Your order #1234 is on the way.",
  "type": 1,
  "data": "{\"orderId\": 1234,\"link\":\"/orders/1234\"}"
}
```

> `type`: `0=Info, 1=Success, 2=Warning, 3=Error`

#### 🛰️ SignalR Hub (JS Client Example)

```javascript
const hub = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7084/hubs/notification?access_token=" + JWT_TOKEN)
  .build();

hub.on("ReceiveNotification", (notif) => console.log("Notification!", notif));
await hub.start(); // When connection is established user auto-joins their own group
```

***

### 🧾 GROUP G — Audit Entries / Change History (2)

**Prefix:** `api/v1/audit-entries`\
**Role:** `Admin \| SuperAdmin`

| #  | Method | Route                                            | Description                                                          |
| -- | ------ | ------------------------------------------------ | -------------------------------------------------------------------- |
| G1 | `GET`  | `/entity/{entityName}/{entityId}?page=1&size=20` | **Single entity** change history (e.g. `AppUser`, `AppRole`)         |
| G2 | `GET`  | `/user/{changedByUserId}?page=1&size=20`         | **A user's** ALL changes made                                        |

Each record: `ChangeType = Added / Modified / Deleted / Restored` + `OldValues` + `NewValues` (JSON diff).

***

### 🔄 GROUP H — Soft Delete Restore (2)

**Prefix:** `api/v1/soft-restore`\
**Role:** `Admin \| SuperAdmin`

| #  | Method | Route                                      | Idem | Description                                                            |
| -- | ------ | ------------------------------------------ | ---- | ---------------------------------------------------------------------- |
| H1 | `GET`  | `/deleted/{entityTypeName}?page=1&size=20` | ❌    | List deleted records (`appuser\|user`, `approle\|role`)                |
| H2 | `POST` | `/restore`                                 | ✅    | Restore deleted record (`Status: Deleted → Active`)                    |

```json
// Restore (H2) Request
{
  "entityTypeName": "appuser",
  "entityId": "<deleted record GUID>"
}
```

***

### 📝 GROUP I — System Logs & User Activities (2)

**Prefix:** `api/v1/system-logs` · **Auth:** 🔑 Bearer JWT

| #  | Method | Route                                   | Description                                                                                                   |
| -- | ------ | --------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| I1 | `GET`  | `/?level=Error&page=1&size=20`          | System logs (Serilog → DB). `level`: Information/Warning/Error/Fatal                                         |
| I2 | `GET`  | `/activities?type=Login&page=1&size=20` | User activities. `type`: Login/Logout/FailedLogin/PasswordChange/ProfileUpdate/RoleAssigned...                |

***

### 🏥 GROUP J — Health / Monitoring (4+2)

**Prefix:** `api/v1/health` · **Auth:** 🔓 AllowAnonymous

| #  | Method | Route (Endpoint) | Description                                                | Cloud MW Alternative                   |
| -- | ------ | ---------------- | ---------------------------------------------------------- | -------------------------------------- |
| J1 | `GET`  | `/live`          | Liveness — is just the HTTP pipeline running?              | `/health/live` (MapHealthChecks)       |
| J2 | `GET`  | `/ready`         | Readiness — check **DB + SMTP + Hangfire** connections     | `/health/ready` (Custom JSON Writer)   |
| J3 | `GET`  | `/detailed`      | Detailed — all Health records + Exception/Data             | -                                      |
| J4 | `GET`  | `/metrics`       | Info: Prometheus endpoint `/metrics`                       | -                                      |

#### Prometheus Real Scraping Endpoint

***

### 🧪 GROUP K — V2 API Test (1 Endpoint)

**Prefix:** `api/v2/test`

| Method | Route    | Description                                              |
| ------ | -------- | -------------------------------------------------------- |
| `GET`  | `/dummy` | API Versioning v2 working test (Returns v2.0 JSON)       |

***

### 🎨 GROUP L — UI Dashboards

**Only** **`env.IsDevelopment()`** → disabled in production (except Hangfire).

| URL                        | What                                                                               | Access                                                        |
| -------------------------- | ---------------------------------------------------------------------------------- | ------------------------------------------------------------- |
| `/swagger`                 | Swagger UI v1 + v2 (OpenAPI)                                                       | Development (Authorize button + Bearer for auth)              |
| `/swagger/v1/swagger.json` | v1 OpenAPI JSON                                                                    | Development                                                   |
| `/swagger/v2/swagger.json` | v2 OpenAPI JSON                                                                    | Development                                                   |
| `/scalar`                  | 🎨 Scalar UI — Modern-looking API docs (v1)                                        | Development                                                   |
| `/scalar/v2`               | Scalar UI (v2)                                                                     | Development                                                   |
| **`/hangfire`**            | ⏱️ Hangfire Dashboard: Queue, Job, Retries, Recurring (nightly-cleanup 02:00 UTC)  | 🔐 **SuperAdmin Role** (All environments, custom auth filter) |
| `/api/v1/odata/$metadata`  | OData v4 EDM Model v1                                                              | AllowAnonymous                                                |
| `/api/v2/odata/$metadata`  | OData v4 EDM Model v2                                                              | AllowAnonymous                                                |

***

## 🛡️ Security Layers (Middleware & Filter)

| Layer                         | How it Works                                                                                                        |
| ----------------------------- | ------------------------------------------------------------------------------------------------------------------- |
| **SecurityStampMiddleware**   | On every request matches `JWT SecurityStamp` ↔ `DB SecurityStamp`; if not matched JWT is invalid (for LogoutAllDevices) |
| **IdempotencyEndpointFilter** | On POST/PUT uses `X-Idempotency-Key` → if same key comes again returns cached result from DB                        |
| **3-Layer Rate Limit**        | `AuthFixedWindow` (10/10min), `GlobalSliding` (100/1min), `IPConcurrency` (200/30min)                              |
| **CORS Secure-by-Default**    | `AllowCredentials = false` + empty `AllowedOrigins` → to be filled for Admin/Frontend                              |
| **Upload Path Traversal**     | `Path.GetFullPath` normalize + `StartsWith(WebRootPath)` → files outside root blocked                              |
| **Production Seed Off**       | SuperAdmin seed runs **only in Development** → no known password in Production                                     |
| **Upload Whitelist**          | Containers outside `avatars / documents / temp / exports` → 400                                                    |

***

## 🏭 Production Deployment Checklist (MANDATORY)

***

## 📌 Total Summary

| Category                                                | Count                      |
| ------------------------------------------------------- | -------------------------- |
| 🔐 Auth Endpoint                                        | **12**                     |
| 🛠️ User Management                                     | **5**                      |
| 🏷️ Roles + Permissions                                 | **6**                      |
| 👤 User Profiles                                        | **3**                      |
| 💾 File Storage                                         | **2**                      |
| 🔔 Notifications + SignalR Hub                          | **4 + 1**                  |
| 🧾 Audit Entries                                        | **2**                      |
| 🔄 Soft Restore                                         | **2**                      |
| 📝 System Logs + Activities                             | **2**                      |
| 🏥 Health + Monitoring                                  | **4 + 2 Middleware Paths** |
| 🧪 V2 Dummy                                             | **1**                      |
| 🎨 UI Dashboard (Swagger/Scalar/Hangfire/OData/Metrics) | **8**                      |
| **TOTAL**                                               | **43+ Endpoints**          |

***

## 📄 License & Documents
