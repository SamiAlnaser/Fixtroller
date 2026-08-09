<div align="center">

# 🔧 Fixtroller

### Maintenance Request Management System

A backend system for managing maintenance requests, technician assignments, maintenance workflows, notifications, reports, and AI-assisted services inside an organization.

<br>

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-9.0-512BD4?logo=dotnet&logoColor=white)
![Entity Framework Core](https://img.shields.io/badge/Entity_Framework_Core-9.0.11-512BD4)
![SQL Server](https://img.shields.io/badge/SQL_Server-Database-CC2927?logo=microsoftsqlserver&logoColor=white)
![C#](https://img.shields.io/badge/C%23-Language-239120?logo=csharp&logoColor=white)
![Architecture](https://img.shields.io/badge/Architecture-3--Tier-3776AB)
![JWT](https://img.shields.io/badge/Auth-JWT-000000?logo=jsonwebtokens&logoColor=white)
![OpenAI](https://img.shields.io/badge/AI-OpenAI-412991?logo=openai&logoColor=white)

</div>

---

## 📖 About The Project

**Fixtroller** is a graduation project designed to manage maintenance requests inside organizations.

The system provides a centralized backend API that allows employees to submit maintenance requests, maintenance managers to review and assign them, and technicians to process assigned tasks while keeping the entire workflow tracked.

Fixtroller supports multiple technician-assignment strategies, role-based permissions, notifications, reporting, localization, file uploads, maintenance statistics, and AI-assisted functionality.

The backend is built using **ASP.NET Core 9**, **Entity Framework Core**, and **Microsoft SQL Server**, following a layered **3-Tier Architecture**.

---

## ✨ Features

- 🔐 Authentication and authorization using **ASP.NET Core Identity + JWT**
- 👥 Role-based access control
- 📝 Create and manage maintenance requests
- 🔧 Assign technicians to maintenance requests
- 👨‍🔧 Support multiple technician assignment modes
- 📊 Maintenance dashboards and statistics
- 📈 KPI and performance reports
- 📄 PDF report generation
- 🔔 In-app notifications
- 📧 Email notifications
- 🤖 AI assistant integration using OpenAI
- 📢 Announcement management
- 🖼️ Image attachments for maintenance requests
- 📝 Maintenance notes
- ⏱️ Technician work-time tracking
- 🌐 Arabic and English localization
- 🗂️ Problem-type management
- 🛠️ Technician-category management
- 📚 OpenAPI documentation using Scalar
- 🧾 Structured logging using Serilog
- ⚠️ Centralized exception handling

---

## 👥 System Roles

Fixtroller contains four main roles:

| Role | Responsibilities |
|---|---|
| **Employee** | Creates maintenance requests, tracks their progress, views notifications and announcements, and manages personal requests. |
| **Technician** | Views assigned maintenance requests, performs maintenance work, adds notes, updates task progress, and tracks work time. |
| **Maintenance Manager** | Reviews requests, assigns technicians, manages maintenance workflows, monitors progress, and controls maintenance operations. |
| **Admin** | Manages users, roles, technicians, categories, problem types, reports, announcements, statistics, and system administration. |

---

## 🔧 Maintenance Request Workflow

A maintenance request can move through several states during its lifecycle:

```text
Submitted
    ↓
Processing
    ↓
Manager Review
    ↓
Processed
    ↓
Completed
```

The system also supports additional request states:

```text
Resources Needed
Cancelled
Reopened
Modified
Not Processed
```

These states allow the system to represent different real-world maintenance scenarios.

---

## 👨‍🔧 Technician Assignment Modes

Fixtroller supports three technician assignment strategies.

### 1. Single

```text
Maintenance Request
        │
        ▼
   Technician
```

A single technician is responsible for the maintenance request.

---

### 2. Team Shared

```text
              Maintenance Request
                      │
          ┌───────────┼───────────┐
          ▼           ▼           ▼
     Technician   Technician   Technician
          │
          ▼
     Lead Technician
```

Multiple technicians work together on the same maintenance task.

A **Lead Technician** is responsible for controlling the main task status.

---

### 3. Parallel Independent

```text
              Maintenance Request
                      │
          ┌───────────┼───────────┐
          ▼           ▼           ▼
       Task 1       Task 2       Task 3
          │           │           │
          ▼           ▼           ▼
     Technician   Technician   Technician
```

Each technician or technician group works on an independent task.

Each task can have its own status while remaining connected to the same maintenance request.

---

## 🏗️ Architecture

Fixtroller follows a **3-Tier Architecture**.

```text
Fixtroller
│
├── Fixtroller.PL
│   └── Presentation Layer
│
├── Fixtroller.BLL
│   └── Business Logic Layer
│
└── Fixtroller.DAL
    └── Data Access Layer
```

### Presentation Layer — `Fixtroller.PL`

Responsible for:

- API Controllers
- Authentication configuration
- Authorization
- Dependency Injection
- Localization
- Email infrastructure
- Notification infrastructure
- Global exception handling
- OpenAPI / Scalar configuration
- Serilog configuration
- Static files
- HTTP request pipeline

---

### Business Logic Layer — `Fixtroller.BLL`

Responsible for:

- Maintenance request business logic
- Technician services
- Authentication services
- User services
- Problem type services
- Technician category services
- Notification services
- Announcement services
- AI services
- Metrics and statistics
- Report generation
- Mapping
- File services

---

### Data Access Layer — `Fixtroller.DAL`

Responsible for:

- Entity Framework Core
- Database context
- Domain entities
- DTOs
- Repositories
- Unit of Work
- Database migrations
- Data seeding utilities
- SQL Server integration

---

## 🧰 Technology Stack

| Technology | Usage |
|---|---|
| **C#** | Main programming language |
| **.NET 9** | Application runtime |
| **ASP.NET Core 9** | Web API framework |
| **Entity Framework Core 9.0.11** | ORM |
| **Microsoft SQL Server** | Relational database |
| **ASP.NET Core Identity** | User and role management |
| **JWT Bearer Authentication** | API authentication |
| **OpenAI .NET SDK** | AI assistant integration |
| **QuestPDF** | PDF report generation |
| **MailKit** | Email sending |
| **Serilog** | Application logging |
| **Scalar** | Interactive API documentation |
| **OpenAPI** | API specification |
| **Mapster** | Object mapping |
| **ImageSharp** | Image processing |

---

## 📁 Project Structure

```text
Fixtroller/
│
├── Fixtroller.PL/
│   ├── Areas/
│   │   ├── Admin/
│   │   ├── Employee/
│   │   ├── Technician/
│   │   ├── MaintenanceManager/
│   │   └── Identity/
│   │
│   ├── GlobalException/
│   ├── Resources/
│   ├── Services/
│   ├── wwwroot/
│   └── Program.cs
│
├── Fixtroller.BLL/
│   ├── Helpers/
│   ├── Mapping/
│   ├── Reports/
│   └── Services/
│
├── Fixtroller.DAL/
│   ├── Data/
│   │   ├── DTOs/
│   │   └── Migrations/
│   │
│   ├── Entities/
│   ├── Repositories/
│   └── Utils/
│
├── Fixtroller.sln
│
└── README.md
```

---

## 🌐 Main API Areas

The API is organized according to system roles and shared services.

```text
Admin
Employee
Technician
MaintenanceManager
Identity
Notifications
Profile
Reports
Diagnostics
```

Major API modules include:

```text
Authentication
Users
Roles
Maintenance Requests
Technician Assignments
Technicians
Technician Categories
Problem Types
Announcements
Notifications
Reports
Statistics
AI Chat
Profile Management
```

---

## 📊 Reports

Fixtroller provides several maintenance reports and performance reports.

Examples include:

- Maintenance department report
- Requests by period
- Single maintenance request report
- Technician performance report
- Technician category performance report
- Request KPI report
- Maintenance duration by problem type

PDF reports are generated using **QuestPDF**.

---

## 🤖 AI Assistant

Fixtroller includes an AI assistant powered by the **OpenAI API**.

The AI functionality is integrated into the backend and can provide role-aware assistance depending on the authenticated user.

AI endpoints are available for different system roles, including:

```text
Admin
Employee
Technician
Maintenance Manager
```

API credentials must never be committed directly to the repository.

---

## 🔔 Notifications

The system supports in-app notifications for important maintenance events.

Notifications can be generated when actions occur such as:

```text
Maintenance request updates
Technician assignments
Request status changes
Manager review
Announcements
Maintenance workflow events
```

The project also includes an email notification background service.

---

## 📧 Email Notifications

Fixtroller uses **MailKit** for SMTP email delivery.

Email configuration should be stored securely using:

```text
.NET User Secrets
Environment Variables
Production Secret Management
```

Do not commit SMTP passwords or email credentials to GitHub.

---

## 🌍 Localization

Fixtroller supports two languages:

```text
Arabic  → ar
English → en
```

Arabic is currently configured as the default application culture.

Localization resources are stored inside:

```text
Fixtroller.PL/Resources
```

---

## 📈 Dashboards & Metrics

Fixtroller contains dashboard and statistics services for different system roles.

Examples include:

```text
Employee Dashboard
Technician Dashboard
Maintenance Manager Dashboard
Admin Statistics
Technician Performance
Maintenance Request Statistics
Charts and KPIs
```

---

## 📚 API Documentation

The project uses:

```text
OpenAPI
+
Scalar
```

When the project runs in the **Development** environment, API documentation is automatically enabled.

The current development launch profiles use:

```text
https://localhost:7127
```

and

```text
http://localhost:5144
```

The browser is configured to open the Scalar API documentation during development.

---

## ⚙️ Requirements

Before running the project, make sure you have:

```text
.NET 9 SDK
SQL Server
Visual Studio 2022
Git
```

---

## 🚀 Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/SamiAlnaser/Fixtroller.git
```

Move into the project directory:

```bash
cd Fixtroller
```

---

### 2. Restore NuGet Packages

```bash
dotnet restore Fixtroller.sln
```

---

### 3. Configure the Database

The project uses **Microsoft SQL Server**.

Configure your connection string using `.NET User Secrets` or another secure configuration method.

Example:

```bash
dotnet user-secrets set "ConnectionStrings:DevConnection" "YOUR_SQL_SERVER_CONNECTION_STRING" --project Fixtroller.PL
```

---

### 4. Configure JWT

Fixtroller requires a secret key for JWT authentication.

Example:

```bash
dotnet user-secrets set "jwtOptions:SecretKey" "YOUR_LONG_RANDOM_SECRET_KEY" --project Fixtroller.PL
```

---

### 5. Configure OpenAI

If you want to use AI functionality:

```bash
dotnet user-secrets set "OpenAI:ApiKey" "YOUR_OPENAI_API_KEY" --project Fixtroller.PL
```

Add the required model configuration according to the application's AI settings.

---

### 6. Configure Email

For email notifications, configure the SMTP settings securely.

Example:

```bash
dotnet user-secrets set "Email:SmtpHost" "YOUR_SMTP_HOST" --project Fixtroller.PL
dotnet user-secrets set "Email:SmtpPort" "587" --project Fixtroller.PL
dotnet user-secrets set "Email:UserName" "YOUR_EMAIL_USERNAME" --project Fixtroller.PL
dotnet user-secrets set "Email:Password" "YOUR_EMAIL_PASSWORD" --project Fixtroller.PL
dotnet user-secrets set "Email:From" "YOUR_FROM_EMAIL" --project Fixtroller.PL
```

---

## 🗄️ Database Migrations

The project uses **Entity Framework Core Migrations**.

Move to the Presentation Layer:

```bash
cd Fixtroller.PL
```

Restore the local Entity Framework tool:

```bash
dotnet tool restore
```

Apply migrations:

```bash
dotnet ef database update --project ../Fixtroller.DAL --startup-project .
```

Then return to the project root:

```bash
cd ..
```

---

## ▶️ Run The Project

From the repository root:

```bash
dotnet run --project Fixtroller.PL
```

The development server will run using the configured launch profile.

Example URLs:

```text
https://localhost:7127
http://localhost:5144
```

---

## 🔐 Security

Fixtroller uses several security mechanisms:

```text
ASP.NET Core Identity
JWT Bearer Authentication
Role-Based Authorization
Password Hashing
Protected API Endpoints
Centralized Exception Handling
```

Sensitive data such as the following should never be committed to GitHub:

```text
Database passwords
JWT secret keys
OpenAI API keys
SMTP passwords
Production connection strings
```

Use `.NET User Secrets`, environment variables, or a secure secret-management solution instead.

---

## 🧾 Logging

Fixtroller uses **Serilog** for structured application logging.

Logs are written to the console during development.

Error logs are also written to:

```text
Fixtroller.PL/Logs/
```

The application keeps rolling error log files for easier debugging and diagnostics.

---

## ⚠️ Exception Handling

The backend uses centralized exception handling based on ASP.NET Core:

```text
Exception Handler
+
Problem Details
```

This provides consistent API error responses and simplifies error management.

---

## 🖼️ File & Image Handling

Maintenance requests can include image attachments.

The backend uses **ImageSharp** for image processing and provides file-management services for uploaded content.

Static files are served through the Presentation Layer.

---

## 🛠️ Development Notes

The project is currently under active development as a graduation project.

Database migrations and seed-data utilities are available in the project.

Automatic migration and seeding code currently exists in `Program.cs` but is disabled, allowing database migration to be controlled manually during development.

CORS is currently configured with a permissive development policy and should be restricted before production deployment.

---

## 🔄 Current Technology Version

The current repository version targets:

```text
.NET 9
ASP.NET Core 9
Entity Framework Core 9.0.11
SQL Server
```

If the project is migrated to **.NET 10 / EF Core 10**, this README should be updated accordingly.

---

## 🎓 Graduation Project

Fixtroller is being developed as a graduation project focused on improving the process of managing maintenance requests inside organizations.

The system aims to provide a structured workflow between:

```text
Employees
    ↓
Maintenance Management
    ↓
Technicians
    ↓
Manager Review
    ↓
Request Completion
```

while providing centralized tracking, notifications, reporting, and administrative control.

---

<div align="center">

### 🔧 Fixtroller

**Maintenance Request Management System**

Built with **ASP.NET Core • Entity Framework Core • SQL Server**

</div>
