# FirstGIG — Backend API 🚀

> **The modern freelancing platform tailored for the Egyptian market.**  
> Built with **.NET 10**, **Modular Monolith**, **Clean Architecture**, and **CQRS**.

---

## 🏛️ Architecture Overview

FirstGIG is architected as a **Modular Monolith** where each business domain is an isolated module adhering strictly to **Clean Architecture** (Domain ➔ Application ➔ Infrastructure ➔ API) with **CQRS** (Command Query Responsibility Segregation) via MediatR.

```
                                FirstGIG Backend
                                        │
                                 Modular Monolith
                                        │
        ┌───────────────┬───────────────┴───────────────┬───────────────┐
        │               │                               │               │
        ▼               ▼                               ▼               ▼
     Identity        Profiles                         Jobs          Payments
      Module          Module                         Module          Module
        │               │                               │               │
   ┌────┼────┐     ┌────┼────┐                     ┌────┼────┐     ┌────┼────┐
   ▼    ▼    ▼     ▼    ▼    ▼                     ▼    ▼    ▼     ▼    ▼    ▼
  API App Domain  API App Domain                  API App Domain  API App Domain
        │               │                               │               │
        └───────────────┴───────────────┬───────────────┴───────────────┘
                                        │
                                 BuildingBlocks
                         (Domain, Application, Infra)
                                        │
                                        ▼
                             SQL Server 2022 / Express
```

---

## 📂 Project Structure

```
FirstGIG/
├── FirstGIG.slnx                                    # .NET 10 Solution File
├── Directory.Build.props                            # Central MSBuild Properties
├── Directory.Packages.props                         # Central Package Management (CPM)
├── .gitignore
│
├── src/
│   ├── Host/
│   │   └── FirstGIG.Host/                           # Web API Host (wires all modules, Swagger, CORS, Serilog)
│   │
│   ├── BuildingBlocks/                              # Shared cross-cutting abstractions
│   │   ├── FirstGIG.BuildingBlocks.Domain/          # Entity, AggregateRoot, ValueObject, Result<T>, Error
│   │   ├── FirstGIG.BuildingBlocks.Application/     # ICommand, IQuery, ValidationBehavior, LoggingBehavior
│   │   └── FirstGIG.BuildingBlocks.Infrastructure/  # Shared infrastructure helpers
│   │
│   └── Modules/
│       └── Identity/                                # Identity & Auth Module
│           ├── FirstGIG.Identity.Domain/            # User aggregate, RefreshToken, Email VO, Enums, Errors
│           ├── FirstGIG.Identity.Application/       # Register, Login, RefreshToken, VerifyEmail, ForgotPassword, ResetPassword
│           ├── FirstGIG.Identity.Infrastructure/    # IdentityDbContext, JwtService, BCrypt, MailKit, UserRepository
│           └── FirstGIG.Identity.Api/               # AuthController (6 endpoints)
│
└── tests/
    └── FirstGIG.Identity.UnitTests/                 # xUnit, FluentAssertions, NSubstitute
```

---

## 🛠️ Tech Stack & Key Libraries

| Technology | Version / Tool | Purpose |
|---|---|---|
| **Framework** | .NET 10.0 (C# 13 / Latest) | Core Runtime & Framework |
| **Database** | SQL Server 2022 / Express | Relational Database |
| **ORM** | Entity Framework Core 10 | Object-Relational Mapper |
| **CQRS Dispatch** | MediatR 12 | Command/Query Pipeline |
| **Validation** | FluentValidation 11 | Request Validation Pipeline |
| **Mapping** | AutoMapper 12 | Entity ↔ DTO Mapping |
| **Authentication** | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) | Stateless Token Auth |
| **Password Hashing** | BCrypt.Net-Next | Secure Password Hashing (WorkFactor 12) |
| **Email Service** | MailKit & MimeKit | Verification & Password Reset Emails |
| **Logging** | Serilog (Console + Rolling File) | Structured Logging |
| **API Docs** | Swashbuckle / Swagger OpenAPI | Interactive API Documentation |

---

## 🚀 Getting Started

### 1. Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (`10.0.x` or higher)
- [SQL Server 2022 Express](https://www.microsoft.com/sql-server/sql-server-downloads) (or LocalDB)
- Optional: VS Code with `SQL Server (mssql)` extension or SSMS

### 2. Clone the Repository
```bash
git clone https://github.com/nadaashraf25003/FirstGig_Back.git
cd FirstGig_Back
```

### 3. Database Setup & Migrations
Ensure SQL Server Express is running (`.\SQLEXPRESS`), then apply migrations:

```powershell
dotnet ef database update --project src\Modules\Identity\FirstGIG.Identity.Infrastructure --startup-project src\Host\FirstGIG.Host
```
> *Note: In development, migrations are also automatically applied upon server startup.*

### 4. Run the API (with Hot Reload)
```powershell
dotnet watch --project src\Host\FirstGIG.Host\FirstGIG.Host.csproj
```

The API will start listening on:
- **Swagger UI:** [http://localhost:5000](http://localhost:5000) *(or [https://localhost:7222](https://localhost:7222))*

---

## 📡 API Endpoints (Identity Module)

| Method | Route | Description | Auth Required |
|---|---|---|---|
| `POST` | `/api/auth/register` | Register a new user (Freelancer or Client) | ❌ No |
| `POST` | `/api/auth/login` | Authenticate with email & password | ❌ No |
| `POST` | `/api/auth/refresh` | Rotate and issue new access token via refresh token | ❌ No |
| `POST` | `/api/auth/verify-email` | Verify user account with email token | ❌ No |
| `POST` | `/api/auth/forgot-password` | Request password reset token | ❌ No |
| `POST` | `/api/auth/reset-password` | Set new password using reset token | ❌ No |

---

## 🔒 User Roles & Enums

- **User Roles (`UserRole`)**:
  - `1` = `Freelancer`
  - `2` = `Client`
  - `3` = `Admin`
- **Account Status (`AccountStatus`)**:
  - `1` = `Pending` (Awaiting email verification)
  - `2` = `Active`
  - `3` = `Suspended`
  - `4` = `Deactivated`

---

## 🗺️ Roadmap & Upcoming Modules
- [x] **Phase 1: Foundation & Identity** (BuildingBlocks, CQRS Pipeline, JWT Auth, Refresh Tokens, Email Verification)
- [ ] **Phase 2: Profiles** (Freelancer Skills/Portfolios & Client Organization Profiles)
- [ ] **Phase 3: Jobs & Proposals** (Job Posting, Proposal Submission, Milestones)
- [ ] **Phase 4: Contracts & Payments** (Paymob / Fawry / Vodafone Cash Escrow & Wallet)
- [ ] **Phase 5: Messaging & Notifications** (SignalR Realtime Chat)

---

## 👥 Contributors
- **Mohad Mohamed**
- **Nada Ashraf**
