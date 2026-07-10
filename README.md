# MediCare — MedTech Innovators

A web-based clinic management system that connects patients, doctors, and administrators. Patients can search for approved doctors and book appointments; doctors manage their working hours, review daily schedules, and maintain patient medical records with prescriptions and documents; administrators oversee accounts, appointments, specializations, and reporting.

<p align="left">
  <img alt=".NET" src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white" />
  <img alt="ASP.NET Core MVC" src="https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?logo=dotnet&logoColor=white" />
  <img alt="EF Core" src="https://img.shields.io/badge/EF%20Core-8.0.8-512BD4" />
  <img alt="SQL Server" src="https://img.shields.io/badge/SQL%20Server-CC2927?logo=microsoftsqlserver&logoColor=white" />
  <img alt="Bootstrap" src="https://img.shields.io/badge/Bootstrap-5-7952B3?logo=bootstrap&logoColor=white" />
  <img alt="License" src="https://img.shields.io/badge/license-none-lightgrey" />
</p>

> **Note:** The repository does not contain a license file. See [License](#license).

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Database](#database)
- [Running the Project](#running-the-project)
- [Build](#build)
- [Testing](#testing)
- [API / Routes](#api--routes)
- [Authentication & Authorization](#authentication--authorization)
- [Screenshots](#screenshots)
- [Deployment](#deployment)
- [Contributing](#contributing)
- [License](#license)
- [Authors](#authors)
- [Notes on Documentation Gaps](#notes-on-documentation-gaps)

---

## Features

The feature set below is derived directly from the controllers, services, and views in the codebase.

### Accounts & Roles
- Self-service registration for **Patient** and **Doctor** accounts (`AccountController`).
- Cookie-based login/logout via ASP.NET Core Identity, with role-based redirects to the appropriate dashboard.
- Three roles — **Admin**, **Doctor**, **Patient** — seeded automatically on startup, along with a default admin account.

### Patients
- View and edit personal profile (blood type, emergency contact, allergies, birth date).
- Search and filter **approved** doctors by name, specialization, and location.
- Book, reschedule, and cancel appointments.
- View own appointments and prescription history.
- Download medical documents attached to their records.

### Doctors
- Manage weekly working hours (add / update / delete per day of week).
- View a daily appointment list for a chosen date.
- Approve or reject pending appointment requests.
- Create medical records for patients (diagnosis, symptoms, treatment plan, notes).
- Add prescriptions (medications) and upload/remove medical documents on a record.
- Print prescriptions.

### Administrators
- Dashboard with a summary of system data.
- Manage doctors (create, edit, approve/delete) and patients (create, edit, delete).
- View and filter all appointments (by status, date, and ordering, with pagination) and update their status.
- Browse the specialization catalogue and per-specialization doctor lists.
- View all registered accounts and system reports.

### Cross-Cutting
- **Appointment engine** with working-hours validation, 30-minute slot logic, double-booking / conflict detection, and automatic **New vs. Follow-Up** classification based on prior completed visits.
- **In-app notifications** (e.g., doctor approval, appointment confirmed/cancelled).
- **Medical document uploads** stored on the server file system under `wwwroot/Uploads/MedicalDocuments`.
- Server-side validation, including a custom `MaxAge` data-annotation attribute.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Language | C# |
| Runtime / Target Framework | .NET 8.0 (`net8.0`) |
| Web framework | ASP.NET Core MVC (Razor Views) |
| Authentication | ASP.NET Core Identity (`Microsoft.AspNetCore.Identity.EntityFrameworkCore` 8.0.8) |
| ORM | Entity Framework Core 8.0.8 |
| Database | Microsoft SQL Server (`Microsoft.EntityFrameworkCore.SqlServer` 8.0.8) |
| Frontend | Razor views + Bootstrap 5, jQuery |
| Client validation | jQuery Validation + jQuery Validation Unobtrusive |
| Secrets | .NET User Secrets (`Microsoft.Extensions.Configuration.UserSecrets`) |
| Solution format | `.slnx` (XML solution file) |

Package versions above are taken from the `.csproj` files.

---

## Architecture

The solution follows a **layered (N-tier) architecture** split across three projects, with several classic patterns applied:

- **Presentation layer — `MediCare.Web`**: ASP.NET Core MVC controllers, Razor views, and view models. Controllers depend on service and repository interfaces.
- **Business / Service layer — `MediCare.Services`**: application services, DTOs, factories, and validation. Encapsulates business rules such as appointment booking, working-hours checks, reporting, and prescription handling.
- **Data layer — `MediCare.Data`**: EF Core `DbContext` (`MedContext`), entity models, and the persistence abstractions.

**Patterns observed in the code:**

- **Repository Pattern** — a generic `Repository<T> : IRepository<T>` plus specialized repositories (`DoctorRepository`, `PatientRepository`, `AppointmentRepository`, `MedicalRecordRepository`, `AccountRepositories`).
- **Unit of Work** — `IUnitOfWork` / `UnitOfWork` aggregates repositories, exposes a generic `Repository<T>()` accessor, `SaveChangesAsync`, and explicit transaction control (`BeginTransactionAsync` / `CommitTransactionAsync` / `RollbackTransactionAsync`).
- **Dependency Injection** — all repositories, services, and factories are registered in `Program.cs` with scoped lifetimes.
- **Factory Pattern** — `AppointmentFactory` (decides New vs. Follow-Up appointment type) and `NotificationFactory` (builds notification entities).
- **Service Result** — a lightweight `ServiceResult` type (`Success()` / `Failure(error)`) used to return operation outcomes without exceptions.
- **DTO / ViewModel separation** — request/response DTOs in `MediCare.Services/DTO` and presentation view models in `MediCare.Web/ViewModels`.

**High-level dependency flow:**

```
MediCare.Web  ──►  MediCare.Services  ──►  MediCare.Data
   (MVC)              (business logic)        (EF Core + models)
     │                                            ▲
     └────────────────────────────────────────────┘
             (Web also references Data directly)
```

---

## Project Structure

```
MedTech-Innovators/
├── MySolution.slnx                  # Solution file (.slnx format)
├── README.md
├── .gitignore
│
├── MediCare.Data/                   # Data access layer
│   ├── Models/                      # EF Core entities
│   │   ├── ApplicationUser.cs       # Identity user (+ FullName, Gender, profiles)
│   │   ├── Doctor.cs, Patient.cs
│   │   ├── Appointment.cs, WorkingHours.cs
│   │   ├── MedicalRecord.cs, Medication.cs, MedicalDocument.cs
│   │   ├── Specialization.cs, Notification.cs
│   │   ├── MedContext.cs            # DbContext (IdentityDbContext) + seed data
│   │   └── Enum/                    # AppointmentType, Gender, Status
│   ├── Repositories/
│   │   ├── Interfaces/              # IRepository<T>, IUnitOfWork, per-entity repos
│   │   └── Implementations/         # Repository<T>, UnitOfWork, per-entity repos
│   └── Migrations/                  # EF Core migrations (Init + model snapshot)
│
├── MediCare.Services/               # Business / application layer
│   ├── Services/
│   │   ├── Interfaces/              # IAppointmentService, IAdminServices, ...
│   │   └── Implementation/          # AppointmentService, AdminServices, ...
│   │       └── ServiceResult.cs     # Success/Failure result type
│   ├── DTO/                         # Request/response DTOs
│   ├── Factory/                     # AppointmentFactory, NotificationFactory
│   └── Validation/                  # MaxAgeAttribute (custom validation)
│
└── MediCare.Web/                    # Presentation layer (ASP.NET Core MVC)
    ├── Program.cs                   # Composition root: DI, Identity, seeding, pipeline
    ├── appsettings.json             # Connection string, logging, allowed hosts
    ├── appsettings.Development.json
    ├── Properties/launchSettings.json
    ├── Controllers/                 # Account, Admin, Appointment, Doctor, Patient,
    │                                #   MedicalRecord, MedicalDocument, Prescription,
    │                                #   Notification, Home
    ├── ViewModels/                  # Login/Register requests + view models
    ├── Views/                       # Razor views grouped by controller
    └── wwwroot/                     # Static assets
        ├── css/ (site.css, theme.css)
        ├── js/ (site.js)
        ├── lib/ (bootstrap, jquery, jquery-validation[-unobtrusive])
        └── Uploads/MedicalDocuments/  # Uploaded medical document files
```

---

## Prerequisites

- **.NET SDK 8.0** or later (the projects target `net8.0`).
- **Microsoft SQL Server** (any edition — LocalDB, Express, Developer, or a full instance) reachable from the app.
- **Entity Framework Core CLI tools** for running migrations:
  ```bash
  dotnet tool install --global dotnet-ef
  ```
- Optional: Visual Studio 2022 / VS Code / JetBrains Rider.

---

## Installation

```bash
# 1. Clone the repository
git clone git@github.com:marco-tharwat/MedTech-Innovators.git
cd MedTech-Innovators

# 2. Restore dependencies
dotnet restore MySolution.slnx

# 3. Configure the database connection (see Configuration below)

# 4. Apply database migrations
dotnet ef database update --project MediCare.Data --startup-project MediCare.Web

# 5. Run the web application
dotnet run --project MediCare.Web
```

---

## Configuration

### Connection string

The application reads a connection string named **`Default`** from configuration (`Program.cs` → `GetConnectionString("Default")`). It is currently defined in `MediCare.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=localhost;Initial Catalog=MediCareDB;..."
  }
}
```

Update the `Data Source`, credentials, and `Initial Catalog` (database name, currently `MediCareDB`) to match your environment.

> ⚠️ **Security note:** The committed `appsettings.json` contains a plaintext SQL Server username and password. For anything beyond local development, move the connection string out of source control — use **User Secrets** (all three projects already declare a `UserSecretsId`) or environment variables, and rotate any credentials that have been committed.

Set the connection string via User Secrets instead of `appsettings.json`:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "<your-connection-string>" --project MediCare.Web
```

Or via an environment variable:

```bash
export ConnectionStrings__Default="Server=...;Database=MediCareDB;User Id=...;Password=...;TrustServerCertificate=True"
```

### Environment & launch settings

- The environment is controlled by `ASPNETCORE_ENVIRONMENT` (set to `Development` in `launchSettings.json`).
- Default development URL (`http` profile): **http://localhost:5041**.
- IIS Express profile: `http://localhost:65029/` with SSL on port `44368`.
- Logging levels are configured in `appsettings.json` / `appsettings.Development.json`.

### Seeded admin account

On first run, `Program.cs` seeds roles and a default administrator if one does not already exist:

| Field | Value |
|-------|-------|
| Username | `admin` |
| Email | `admin@admin.com` |
| Password | `Admin@123` |
| Role | `Admin` |

> Change this default password immediately in any shared or deployed environment.

---

## Database

- **Engine:** Microsoft SQL Server, accessed through EF Core (`UseSqlServer`).
- **Context:** `MedContext` extends `IdentityDbContext<ApplicationUser>`, so ASP.NET Core Identity tables are created alongside the domain tables.
- **Domain tables:** Doctors, Patients, Specializations, WorkingHours, Appointments, MedicalRecords, MedicalDocuments, Medications, Notifications.
- **Relationships & delete behavior:** Appointments and MedicalRecords are configured with `DeleteBehavior.NoAction` on both patient and doctor foreign keys (records are preserved if a doctor/patient is deleted).
- **Seed data:** A fixed catalogue of **25 medical specializations** (Cardiology, Dermatology, Neurology, etc.) is seeded via `HasData` in `OnModelCreating` with stable IDs.

### Migrations

Migrations live in `MediCare.Data/Migrations` (initial migration: `Init`).

```bash
# Apply existing migrations / create the database
dotnet ef database update --project MediCare.Data --startup-project MediCare.Web

# Add a new migration after model changes
dotnet ef migrations add <MigrationName> --project MediCare.Data --startup-project MediCare.Web
```

> The `.gitignore` contains a `Migrations/` rule; the migration files under `MediCare.Data/Migrations` are nonetheless tracked in the repository.

---

## Running the Project

```bash
# From the repository root
dotnet run --project MediCare.Web
```

Then browse to **http://localhost:5041** and sign in with the seeded admin account, or register a new Patient/Doctor from the registration page.

For an auto-rebuilding development loop:

```bash
dotnet watch --project MediCare.Web run
```

---

## Build

```bash
# Build the whole solution
dotnet build MySolution.slnx

# Build only the web app
dotnet build MediCare.Web/MediCare.Web.csproj

# Publish a release build
dotnet publish MediCare.Web/MediCare.Web.csproj -c Release -o ./publish
```

---

## Testing

No test project is present in the repository. There are currently no automated unit or integration tests to run.

---

## API / Routes

This is a **server-rendered MVC application**, not a REST/JSON API. There is **no Swagger/OpenAPI** integration and no dedicated API base URL — endpoints are MVC controller actions that return Razor views (or file downloads). Routing uses the default convention:

```
{controller=Home}/{action=Index}/{id?}
```

Main route groups by controller:

| Controller | Access | Representative Actions |
|------------|--------|------------------------|
| `Home` | Public | `Index`, `Privacy`, `Error` |
| `Account` | Public | `Register`, `Login`, `Logout` |
| `Doctor` | Mixed | `Index` (Doctor), `Filter` / `Details` (browse), `Create` / `Edit` / `Delete` (Admin) |
| `Patient` | Mixed | `Profile` / `EditProfile` / `Index` (Patient), `Create` / `Edit` / `Delete` / `Details` (Admin) |
| `Appointment` | Authenticated | `Book`, `Reschedule`, `Cancel` (Patient); `DoctorDailyList`, `Approve`, `Reject`, working-hours CRUD (Doctor) |
| `MedicalRecord` | Doctor, Admin | `GetPatientHistory`, `GetRecordDetails`, `CreateMedicalRecord` |
| `MedicalDocument` | Doctor, Admin (download also Patient) | `AddDocument`, `RemoveDocument`, `DownloadDocument` |
| `Prescription` | Doctor, Admin (history: Patient; print: all roles) | `AddMedication`, `RemoveMedication`, `PrintPrescription`, `GetPrescriptionHistory` |
| `Admin` | Admin | `Dashboard`, `ManageDoctors`, `ManagePatients`, `AllAppointments`, `Reports`, `AllRegistered`, `Specialization(s)` |
| `Notification` | Authenticated | `Index` |

---

## Authentication & Authorization

- **Authentication** is handled by **ASP.NET Core Identity** with cookie-based sign-in (`SignInManager.SignInAsync`), including a "Remember me" option. `ApplicationUser` extends `IdentityUser` with `FullName`, `Gender`, optional doctor/patient profiles, and notifications.
- **Registration** creates the Identity user, assigns the chosen role, and provisions the matching `Doctor` (unapproved by default) or `Patient` profile via `AccountRepositories.SetNewAccount`.
- **Roles** — `Admin`, `Doctor`, `Patient` — are seeded at startup (`AccountController.SeedRoles`), together with a default admin user.
- **Authorization** is enforced with `[Authorize]` and `[Authorize(Roles = "...")]` attributes at the controller and action level. Examples:
  - `AdminController` → `[Authorize(Roles = "Admin")]` (class-level).
  - `MedicalRecordController` → `[Authorize(Roles = "Doctor, Admin")]`.
  - `AppointmentController` → `[Authorize]` at class level, with per-action role restrictions (`Patient` for booking, `Doctor` for approvals/working hours).
- **Login redirects** are role-aware: Admin → Admin dashboard, Doctor → Doctor index, Patient → Patient index.
- **Doctor approval gate:** newly registered doctors have `IsApproved = false`; only approved doctors are surfaced in patient-facing search and filtering.

---

## Screenshots

No application screenshots are committed to the repository. Add them under a `docs/` (or `screenshots/`) folder and reference them here, for example:

```markdown
![Admin Dashboard](docs/admin-dashboard.png)
![Book Appointment](docs/book-appointment.png)
```

> The files under `wwwroot/Uploads/MedicalDocuments/` are user-uploaded sample documents, not project screenshots.

---

## Deployment

No deployment configuration (Docker, docker-compose, CI/CD pipelines, or cloud manifests) is present in the repository. A standard ASP.NET Core deployment can be inferred:

1. Provide a production SQL Server connection string via environment variables or a secrets store (do **not** ship credentials in `appsettings.json`).
2. Apply migrations against the target database:
   ```bash
   dotnet ef database update --project MediCare.Data --startup-project MediCare.Web
   ```
3. Publish a release build:
   ```bash
   dotnet publish MediCare.Web/MediCare.Web.csproj -c Release -o ./publish
   ```
4. Host the published output behind a reverse proxy (IIS, Nginx, or Kestrel directly). Ensure the `wwwroot/Uploads/MedicalDocuments` directory is writable for document uploads and consider persistent/shared storage for it.
5. Set `ASPNETCORE_ENVIRONMENT=Production`; the app enables the `/Home/Error` exception handler and HTTPS redirection outside Development.

---

## Contributing

1. Fork the repository and create a feature branch:
   ```bash
   git checkout -b feature/your-feature
   ```
2. Follow the existing layered structure — keep data access in `MediCare.Data`, business logic in `MediCare.Services`, and presentation concerns in `MediCare.Web`.
3. When changing entities, add an EF Core migration and verify `dotnet build MySolution.slnx` succeeds.
4. Do **not** commit secrets or connection strings; use User Secrets or environment variables.
5. Open a pull request against `main` with a clear description of the change.

---

## License

No license file is present in the repository. Absent an explicit license, the work is **All Rights Reserved** by default and cannot be reused or distributed without permission from the authors. Consider adding a `LICENSE` file if the project is intended to be open source.

---

## Authors

Derived from the Git commit history (`git shortlog`):

| Author | Contact |
|--------|---------|
| Marco Tharwat | markotharwat11@gmail.com |
| Seif El-den Karam Salah | sk1536@fayoum.edu.eg |
| Yousef Ali | yousefalinl0@gmail.com |
| Omar Rabea | Omar.Rabea.Div@gmail.com |
| Mostafa Mohammed | mostafa69722@gmail.com |
| Hazem Shaban | hs3833024@gmail.com |

Repository: `git@github.com:marco-tharwat/MedTech-Innovators.git`

---

## Notes on Documentation Gaps

The following items could not be documented because the information is **not present in the repository**. They are listed so the README stays fully evidence-based:

- **No license file** — licensing intent is unknown.
- **No test project** — testing strategy and coverage are undefined.
- **No CI/CD, Docker, or deployment configuration** — deployment steps are inferred from a standard ASP.NET Core workflow, not from repo artifacts.
- **No API documentation / Swagger** — this is an MVC (view-rendering) app, so there is no machine-readable API surface.
- **No screenshots or `docs/` assets** — placeholders are provided instead.
- **No project-level `CLAUDE.md`, wiki, or design docs** — architecture notes above are reconstructed from source code.
- **Committed plaintext database credentials** in `appsettings.json` — flagged as a security concern rather than reproduced here.

**Assumptions:** None. Every statement in this README is based on files, code, or Git history found in the repository. Where information was missing, it is called out explicitly above rather than assumed.