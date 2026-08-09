# Fixtroller

**Fixtroller** is a graduation-project backend for managing maintenance requests inside an organization. It provides a role-based REST API for employees, technicians, maintenance managers, and administrators, covering the full maintenance-request lifecycle from submission and technician assignment to review, completion, notifications, and reporting.

> The current project version targets **.NET 9** and **Entity Framework Core 9.0.11**.

## Features

- Role-based authentication and authorization using **ASP.NET Core Identity + JWT**.
- Four system roles: **Admin**, **MaintenanceManager**, **Technician**, and **Employee**.
- Create, update, track, reopen, complete, and cancel maintenance requests.
- Maintenance-request priorities, problem types, locations, notes, and image attachments.
- Three technician assignment modes:
  - `Single` — one technician is responsible for the request.
  - `TeamShared` — multiple technicians share one task with a lead technician.
  - `ParallelIndependent` — technicians or groups work on independent tasks with separate task statuses.
- Technician categories and problem types with **Arabic / English localization**.
- Technician work-time tracking.
- Announcements with per-user read status.
- In-app notifications and email notifications through a background worker.
- Maintenance dashboards, metrics, KPI endpoints, and reports.
- PDF report generation using **QuestPDF**.
- AI assistant integration using the **OpenAI API**, with behavior adapted to the current user's role.
- Development API documentation using **OpenAPI + Scalar**.
- Structured application logging using **Serilog**.
- Centralized exception handling with ASP.NET Core Problem Details.

## Maintenance Request Workflow

A maintenance request can move through states such as:

`Submitted` → `Processing` → `ManagerReview` → `Completed`

The system also supports additional states including:

- `ResourcesNeeded`
- `Cancelled`
- `Reopened`
- `Modified`
- `Processed`
- `NotProcessed`

For `ParallelIndependent` assignments, each technician/group has its own task status, while the main request status is recalculated according to the progress of the assigned tasks.

## Roles

| Role | Main responsibilities |
|---|---|
| **Employee** | Submit and follow maintenance requests, add information, view announcements, notifications, and personal statistics. |
| **Technician** | View assigned requests, update task/request progress according to permissions, add notes, track work time, and request resources. |
| **MaintenanceManager** | Review requests, assign technicians, manage technician work, update request states, and monitor maintenance performance. |
| **Admin** | Full administrative access, user and role management, technician/category management, reports, metrics, announcements, and system-level controls. |

## Architecture

The solution follows a layered architecture:

```text
Fixtroller.sln
├── Fixtroller.PL   # Presentation / API layer
├── Fixtroller.BLL  # Business logic and application services
└── Fixtroller.DAL  # Data access, entities, repositories, migrations
```

### Fixtroller.PL

Contains:

- API controllers grouped by role/area.
- Authentication and authorization pipeline.
- Dependency injection configuration.
- Localization resources.
- Email and notification infrastructure.
- Global exception handling.
- Scalar/OpenAPI configuration.
- Serilog configuration.

### Fixtroller.BLL

Contains:

- Maintenance-request business logic.
- Technician management logic.
- User and authentication services.
- Announcement and notification services.
- AI chat integration.
- Reporting services.
- Mapping and helper components.

### Fixtroller.DAL

Contains:

- Entity Framework Core `ApplicationDbContext`.
- Domain entities and DTOs.
- Repositories.
- Unit of Work implementation.
- Entity Framework Core migrations.
- Seed-data utilities.

## Technology Stack

| Technology | Usage |
|---|---|
| **ASP.NET Core 9 Web API** | Backend API |
| **C# / .NET 9** | Application platform |
| **Entity Framework Core 9.0.11** | ORM and migrations |
| **SQL Server** | Relational database |
| **ASP.NET Core Identity** | User and role management |
| **JWT Bearer Authentication** | API authentication |
| **Scalar + OpenAPI** | Interactive API documentation |
| **Serilog** | Application and error logging |
| **QuestPDF** | PDF report generation |
| **MailKit** | SMTP email delivery |
| **OpenAI .NET SDK** | AI assistant integration |
| **Mapster** | Object mapping |
| **ImageSharp** | Image processing |

## Requirements

Before running the project, install:

- **.NET 9 SDK**
- **SQL Server** or SQL Server Express/Developer
- **Visual Studio 2022** or another .NET-compatible IDE

The repository also includes a local `dotnet-ef` tool manifest.

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/SamiAlnaser/Fixtroller.git
cd Fixtroller
```

### 2. Restore NuGet packages

```bash
dotnet restore Fixtroller.sln
```

### 3. Restore the local EF Core tool

```bash
cd Fixtroller.PL
dotnet tool restore
cd ..
```

### 4. Configure local secrets

The API requires a database connection string and JWT secret. The AI and email features also require their corresponding credentials.

For development, use **.NET User Secrets** or environment variables instead of committing real credentials to GitHub.

Example:

```bash
dotnet user-secrets set "ConnectionStrings:DevConnection" "YOUR_SQL_SERVER_CONNECTION_STRING" --project Fixtroller.PL
dotnet user-secrets set "jwtOptions:SecretKey" "YOUR_LONG_RANDOM_JWT_SECRET" --project Fixtroller.PL
dotnet user-secrets set "OpenAI:ApiKey" "YOUR_OPENAI_API_KEY" --project Fixtroller.PL
dotnet user-secrets set "OpenAI:Model" "YOUR_MODEL_NAME" --project Fixtroller.PL
```

Optional email configuration:

```bash
dotnet user-secrets set "Email:SmtpHost" "YOUR_SMTP_HOST" --project Fixtroller.PL
dotnet user-secrets set "Email:SmtpPort" "587" --project Fixtroller.PL
dotnet user-secrets set "Email:UserName" "YOUR_SMTP_USERNAME" --project Fixtroller.PL
dotnet user-secrets set "Email:Password" "YOUR_SMTP_PASSWORD" --project Fixtroller.PL
dotnet user-secrets set "Email:From" "YOUR_FROM_EMAIL" --project Fixtroller.PL
```

> Never commit production connection strings, JWT secrets, SMTP passwords, or API keys to the repository.

### 5. Apply database migrations

From the repository root:

```bash
dotnet ef database update --project Fixtroller.DAL --startup-project Fixtroller.PL
```

### 6. Run the API

```bash
dotnet run --project Fixtroller.PL
```

Development URLs from the current launch profile are:

- `https://localhost:7127`
- `http://localhost:5144`

## API Documentation

When the application runs in the `Development` environment, Scalar documentation is enabled.

Open:

```text
https://localhost:7127/Scalar
```

or:

```text
http://localhost:5144/Scalar
```

## Main API Areas

The API controllers are organized around the application's roles and shared services:

```text
Api/Admin/...
Api/MaintenanceManager/...
Api/Technician/...
Api/Employee/...
Api/Notifications/...
Api/Profile/...
Api/Reports/...
```

Major modules include:

- Authentication
- Maintenance Requests
- Technician Assignment
- Technician Categories
- Problem Types
- Users and Roles
- Announcements
- Notifications
- Reports and KPIs
- AI Chat
- Profile Management

## Localization

The API supports:

- Arabic: `ar` — default culture
- English: `en`

Localization resources are stored under:

```text
Fixtroller.PL/Resources
```

## Logging

Fixtroller uses **Serilog** for console and file logging.

Error logs are written to:

```text
Fixtroller.PL/Logs/
```

## Database

The project uses **SQL Server** with Entity Framework Core.

Main persisted data includes:

- Users and roles
- Maintenance requests
- Technician assignments
- Maintenance notes
- Request images
- Work-time entries
- Problem types
- Technician categories
- Announcements and read states
- Notifications
- AI chat settings

## Development Notes

- Database migration/seeding code exists in the project, but automatic migration/seeding is currently commented out in `Program.cs`.
- CORS is currently configured with a permissive development-style policy. Review this before production deployment.
- Secrets should be kept outside tracked `appsettings*.json` files.
- The current project version targets `.NET 9`; update this README together with the project if the solution is later migrated to `.NET 10` / EF Core 10.

## Project Status

Fixtroller is being developed as a **graduation project** for managing organizational maintenance requests and maintenance-team workflows.

---

Built with ASP.NET Core, Entity Framework Core, and SQL Server.
