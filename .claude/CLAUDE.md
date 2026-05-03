# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build PlaygroundDashboard.csproj

# Run (dev)
dotnet run --project PlaygroundDashboard.csproj

# EF Core migrations
dotnet ef migrations add <MigrationName> --project PlaygroundDashboard.csproj
dotnet ef database update --project PlaygroundDashboard.csproj

# Publish
dotnet publish PlaygroundDashboard.csproj -c Release
```

No test project exists in this repository.

## Architecture

**ASP.NET Core 10 + vanilla JS SPA.** There is no frontend build step — the entire UI lives in a single file, `wwwroot/index.html`, served as a fallback route. All backend routes are under `/api/`.

### Request flow

1. Browser loads `index.html` (served by `MapFallbackToFile`).
2. JS calls REST endpoints (`/api/children`, `/api/account/*`, `/api/settings`).
3. SignalR hub at `/hubs/playground` pushes `ChildAdded`, `ChildUpdated`, `ChildRemoved` events to all connected clients so every open tab (admin + lobby) stays in sync without polling.
4. A 10-second polling fallback (`pollServer`) catches changes made directly to the database.

### Frontend state machine (`wwwroot/index.html`)

The SPA has no framework. Rendering is driven by three global flags:

| Flag | Values | Effect |
|---|---|---|
| `VIEW` | `'admin'` / `'lobby'` | Set from `?view=` query param at load; never changes |
| `isAuth` | `true` / `false` | Switches between login forms and admin panel |
| `adminPage` | `'main'` / `'settings'` | Switches between child list and settings editor |

`fullRender()` re-renders the entire `#app` div based on these flags. Targeted DOM mutations (`updateAdminTimers`, `updateLobbyTimers`) run on a 1-second tick to avoid full re-renders for timer/progress-bar updates.

### Two views

- **Admin** (`?view=admin`, default) — authenticated, manages children. Sidebar for adding, main area lists active children with timers, edit/checkout/print actions.
- **Lobby** (`?view=lobby`) — public display screen, no auth required, shows real-time child countdown timers.

### Key backend files

| File | Purpose |
|---|---|
| `Program.cs` | Service registration, middleware pipeline, `SeedAsync` (seeds admin user + default `PlaygroundSettings` on first run) |
| `Data/AppDbContext.cs` | EF Core context — `Children` and `Settings` tables |
| `Controllers/ChildrenController.cs` | CRUD for active children; broadcasts SignalR events on each mutation |
| `Controllers/SettingsController.cs` | GET (public) / PUT (authorized) for the single `PlaygroundSettings` row |
| `Controllers/AccountController.cs` | Cookie auth: login, logout, register, forgot/reset password |
| `Hubs/PlaygroundHub.cs` | Empty SignalR hub (events are sent server-side via `IHubContext`) |
| `Services/EmailService.cs` | SMTP via `SmtpClient`; used only for password-reset emails |

### Database

SQL Server LocalDB (`.\SQLEXPRESS`, database `PlaygroundDashboard`). EF Core migrations are in `Migrations/`. `Program.cs` calls `MigrateAsync()` on startup so pending migrations apply automatically.

`PlaygroundSettings` always has exactly one row (upserted in `SettingsController.Update` and seeded in `SeedAsync`).

`Child.IsActive = false` is a soft delete; the GET endpoint filters `WHERE IsActive = 1`.

### Authentication

Cookie-based (`AddIdentity` + `ConfigureApplicationCookie`). API endpoints return HTTP 401 (not a redirect) when unauthenticated, via the `OnRedirectToLogin` override. Password policy: min 6 chars, no complexity requirements.

### Configuration (`appsettings.json`)

| Key | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Admin:Email` / `Admin:Password` | Seeded admin credentials |
| `Email:*` | SMTP settings for password-reset emails (Gmail by default) |

### Print feature

`printResponsibility(childId)` in `index.html` opens a popup window with a styled A4-like document containing the responsibility text from `appSettings.responsibilityText`, child info, and two signature lines. The text is managed via the admin settings page (`adminPage = 'settings'`).
