---
applyTo: '**'
---
# Camagru — Copilot Chat Instructions

## Project overview
Camagru is a photo-editing web application where authenticated users can capture webcam images, overlay predefined transparent PNG stickers, and share the results in a public gallery. Built with C# .NET using clean architecture.

## Solution structure
```
src/
  Camagru.Domain/         # Entities and repository interfaces only — zero dependencies
  Camagru.Application/    # Use cases, DTOs, service interfaces — depends on Domain only
  Camagru.Infrastructure/ # EF Core, ImageSharp, MailKit, BCrypt — implements Application interfaces
  Camagru.Web/            # ASP.NET Core MVC — controllers, Razor views, vanilla JS
tests/
  Camagru.Tests/          # xUnit tests
```

## Dependency rules (never break these)
- `Domain` has no project or NuGet references
- `Application` references `Domain` only
- `Infrastructure` references `Application` only
- `Web` references `Application` and `Infrastructure` (Infrastructure only for DI registration in Program.cs)
- Controllers must never instantiate Infrastructure classes directly — always go through Application service interfaces

## Tech stack
- **Runtime**: .NET 8, ASP.NET Core MVC
- **ORM**: Entity Framework Core with Npgsql (PostgreSQL)
- **Image processing**: SixLabors.ImageSharp (server-side compositing with alpha channel support)
- **Auth**: ASP.NET Core cookie authentication (hand-rolled, no ASP.NET Identity)
- **Password hashing**: BCrypt.Net-Next
- **Email**: MailKit
- **Database**: PostgreSQL (via Docker)
- **Containerization**: Docker + docker-compose
- **Client-side**: Vanilla JS only (no frameworks, no libraries — browser native APIs only)
- **CSS**: Plain CSS or a CSS-only framework (no JS-dependent CSS frameworks)

## Domain entities
```csharp
User        // Id, Username, Email, PasswordHash, IsConfirmed, ConfirmationToken,
            // ResetToken, ResetTokenExpiry, EmailNotificationsEnabled, CreatedAt
Image       // Id, UserId, FilePath, CreatedAt
Comment     // Id, ImageId, UserId, Content, CreatedAt
Like        // Id, ImageId, UserId
```

## Key NuGet packages
| Project | Package |
|---|---|
| Infrastructure | Microsoft.EntityFrameworkCore |
| Infrastructure | Npgsql.EntityFrameworkCore.PostgreSQL |
| Infrastructure | Microsoft.EntityFrameworkCore.Relational |
| Infrastructure | BCrypt.Net-Next |
| Infrastructure | MailKit |
| Infrastructure | SixLabors.ImageSharp |
| Web | Microsoft.EntityFrameworkCore.Design |
| Web | Microsoft.Extensions.Configuration.EnvironmentVariables |

## Coding conventions
- Use `async/await` throughout — all repository and service methods must be async
- Services in Application layer return result objects or throw domain exceptions — never return raw EF entities to the Web layer
- DTOs live in `Camagru.Application/DTOs/`
- Service interfaces live in `Camagru.Application/Interfaces/`
- Repository interfaces live in `Camagru.Domain/Interfaces/`
- EF Core `AppDbContext` lives in `Camagru.Infrastructure/Persistence/`
- Migrations live in `Camagru.Infrastructure/Persistence/Migrations/`
- Never use `[FromServices]` in views — pass everything through the controller into the ViewModel
- Use `DataAnnotations` for model validation — no FluentValidation, no AutoMapper, no MediatR

## Security requirements (mandatory — all must be implemented)
- Passwords hashed with BCrypt before storage — never store plain text
- All POST endpoints protected with `[ValidateAntiForgeryToken]`
- File uploads validated by MIME type and extension whitelist (images only: jpg, jpeg, png)
- All EF queries use parameterized form (LINQ or `.FromSqlInterpolated`) — no raw string SQL
- XSS: Razor escapes by default — audit any use of `@Html.Raw`
- CSRF: use `@Html.AntiForgeryToken()` in every form
- Editing page and all write actions redirect unauthenticated users to login
- Delete actions verify the requesting user owns the resource before proceeding
- Secrets (DB connection string, SMTP credentials) loaded from environment variables via `.env` — never committed to git

## Authentication flow
- Registration → BCrypt hash → save user with `IsConfirmed = false` + unique `ConfirmationToken` → send confirmation email
- Email confirmation → validate token → set `IsConfirmed = true`
- Login → look up user → BCrypt verify → issue cookie via `HttpContext.SignInAsync`
- Password reset → generate `ResetToken` + expiry → send email → validate token on reset form → BCrypt hash new password
- Logout → `HttpContext.SignOutAsync`

## Image editing flow
1. Client streams webcam via `getUserMedia()` on a `<canvas>` element
2. User selects a superposable overlay image from a list (button disabled until one is selected)
3. On capture: client sends raw webcam frame (base64 or multipart) + overlay ID to server via POST
4. Server composites the two images using ImageSharp (overlay must have alpha channel)
5. Server saves result to `wwwroot/uploads/` and records the path in the DB
6. File upload fallback: allow `<input type="file">` instead of webcam capture

## Gallery rules
- Public — no login required to view
- Paginated: minimum 5 images per page, ordered by `CreatedAt` descending
- Likes and comments require authentication
- One like per user per image (enforce at DB level with unique constraint on `(ImageId, UserId)`)
- On new comment: notify image author by email if `EmailNotificationsEnabled = true`

## Environment variables (loaded from .env)
```
POSTGRES_HOST
POSTGRES_PORT
POSTGRES_DB
POSTGRES_USER
POSTGRES_PASSWORD
SMTP_HOST
SMTP_PORT
SMTP_USER
SMTP_PASSWORD
APP_BASE_URL
```

## Docker
- `docker-compose up --build` must start the full application with zero manual steps
- Two services: `web` (ASP.NET Core) and `db` (postgres:16)
- Migrations should run automatically on startup (`context.Database.MigrateAsync()` in Program.cs)

## What Copilot should never suggest
- ASP.NET Core Identity (too heavy, not needed)
- AutoMapper or MediatR (no PHP equivalent, risky during peer evaluation)
- FluentValidation (same reason)
- Any client-side JS framework or library (React, Vue, Alpine, htmx, jQuery...)
- Raw SQL string concatenation
- `@Html.Raw` unless explicitly requested and sanitized
- Storing tokens or passwords in plain text