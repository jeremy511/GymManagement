# GymManagement

A multi-tenant gym management backend built with **.NET 10 / ASP.NET Core**. It exposes a RESTful API to manage gyms, staff and member accounts, membership plans, scheduled classes with reservations, and payments. The repository also includes a minimal **React + TypeScript (Vite)** frontend scaffold.

## Features

- **Multi-tenancy** – every gym is an isolated tenant. The tenant (`GymId`) is resolved from the JWT `gym_id` claim (falling back to an `X-Gym-Id` header), and EF Core global query filters scope data to the current tenant automatically.
- **Authentication & authorization** – JWT bearer authentication with role-based access control (`Admin`, `Staff`, `Member`). Windows/Negotiate authentication is also registered.
- **Members & memberships** – manage member profiles, membership types (plans), and time-bound memberships. A member's active-membership status is derived automatically.
- **Classes & reservations** – schedule classes with a capacity limit and let members reserve/cancel spots. Business rules prevent overbooking and require an active membership to reserve.
- **Payments** – record member payments (cash, card, transfer) with amount validation and optional external references.
- **Soft deletes** – entities implementing `ISoftDeletable` are flagged as deleted rather than physically removed.
- **API documentation** – native .NET 10 OpenAPI document with a [Scalar](https://github.com/scalar/scalar) UI (with Bearer auth support) in Development.
- **Operational concerns** – structured logging via **Serilog** (console + rolling daily files) and IP **rate limiting** (`AspNetCoreRateLimit`).

## Tech Stack

| Area | Technology |
| --- | --- |
| Runtime | .NET 10 (`net10.0`), SDK `10.0.103` |
| Web | ASP.NET Core, Controllers, OpenAPI / Scalar |
| Persistence | Entity Framework Core 10, SQL Server |
| Patterns | MediatR (CQRS commands/queries), AutoMapper, Result pattern |
| Auth | JWT Bearer, Negotiate, role-based policies |
| Logging | Serilog (Console + rolling File sink) |
| Rate limiting | AspNetCoreRateLimit (in-memory) |
| Frontend | React 19, TypeScript, Vite |

## Architecture

The API follows a **feature-sliced (vertical slice) architecture**. Each feature owns its domain models, commands, queries, and controllers:

```
GymManagement.Api/
├── Features/
│   ├── Auth/          # Registration, login, profile updates, User domain
│   ├── Gyms/          # Gym (tenant) profile management
│   ├── Members/       # Member profiles, Membership entity
│   ├── Memberships/   # Membership types (plans) and memberships
│   ├── Classes/       # Classes and reservations
│   └── Payments/      # Member payments
│       ├── Commands/      # Write operations (MediatR IRequest handlers)
│       ├── Queries/       # Read operations
│       ├── Controllers/   # HTTP endpoints + request/response records
│       └── Domain/        # Aggregate roots / entities
├── Infrastructure/
│   ├── Persistence/   # GymManagementDbContext, DbInitializer, soft delete
│   ├── Tenant/        # Tenant resolution (claim/header), per-tenant providers
│   ├── Mapping/       # AutoMapper profiles
│   └── Common/        # Result<T> helper
├── Services/          # JWT generation, password hashing
├── Shared/Security/   # Roles and custom claim constants
├── Migrations/        # EF Core migrations
└── Program.cs         # Composition root / pipeline configuration
```

## Getting Started

### Prerequisites

- [.NET SDK 10.0.103+](https://dotnet.microsoft.com/download) (see `global.json`)
- SQL Server (or use the provided Docker Compose setup)
- [Node.js](https://nodejs.org/) (only for the frontend)

### Run with Docker Compose (recommended)

This starts both the API and a SQL Server instance. The compose file lives in `GymManagement.Api/`:

```bash
cd GymManagement.Api
docker compose up --build
```

- API: http://localhost:8080
- SQL Server: `localhost:1433` (`sa` / `YourStrong!Passw0rd`)

On startup in Development the API applies EF Core migrations and seeds initial data (see [Seeded data](#seeded-data)).

### Run locally with the .NET CLI

1. Configure the SQL Server connection string. The API reads `ConnectionStrings:DefaultConnection`; set it via `appsettings.json`, user secrets, or an environment variable, e.g.:

   ```bash
   export ConnectionStrings__DefaultConnection="Server=localhost;Initial Catalog=GymManagementDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
   ```

2. (Optional) set a strong JWT signing key — the default fallback is for development only:

   ```bash
   export Jwt__Key="a-strong-secret-signing-key"
   ```

3. Run the API:

   ```bash
   cd GymManagement.Api
   dotnet restore
   dotnet run
   ```

   Default URLs: `http://localhost:5244` and `https://localhost:7192`. The browser opens the Scalar API reference at `/scalar/v1`.

### Frontend (optional)

```bash
cd gymManagement-front
npm install
npm run dev
```

## Configuration

| Setting | Description |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string. |
| `Jwt:Key` | Symmetric signing key for JWT tokens (defaults to a placeholder in development). |

> Note: migrations and data seeding only run automatically when the environment is `Development`.

## Authentication

1. Obtain a token via `POST /api/Auth/login`.
2. Send it on subsequent requests as `Authorization: Bearer <token>`.

Tokens embed the `userId`, `gym_id`, and `role` claims used for tenancy and authorization. Roles are `Admin`, `Staff`, and `Member`.

### Seeded data

In Development, the database is seeded with a sample gym and accounts:

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@irontemple.com` | `Admin123!` |
| Member | `juan.perez@email.com` | `Member123!` |
| Member | `maria.garcia@email.com` | `Member123!` |

## API Overview

All routes are prefixed with `/api`. Most endpoints require authentication; bracketed roles indicate the required role(s).

### Auth (`/api/Auth`)
- `POST /register` *(anonymous)* – register a new gym + admin user
- `POST /login` *(anonymous)* – authenticate and receive a JWT
- `PUT /profile/{userId}` – update a user profile

### Gyms (`/api/gyms`)
- `GET /` – get the current tenant's gym
- `PUT /` *(Admin, Staff)* – update the gym profile

### Members (`/api/members`) *(Admin, Staff)*
- `POST /` – create a member
- `GET /` – list members (search + pagination)
- `GET /{id}` *(any authenticated; members limited to themselves)* – get a member
- `PUT /{id}` – update a member
- `DELETE /{id}` – soft-delete a member

### Membership types (`/api/membership-types`) *(Admin, Staff)*
- `POST /` · `GET /` · `PUT /{id}` · `DELETE /{id}`

### Memberships (`/api/memberships`) *(Admin, Staff)*
- `POST /` – assign a membership to a member
- `GET /` – list memberships
- `GET /{id}` – get a membership
- `GET /member/{memberId}` – list a member's memberships

### Classes (`/api/classes`)
- `POST /` *(Admin, Staff)* – create a class
- `GET /` *(any authenticated)* – list classes
- `GET /{id}` *(any authenticated)* – get a class
- `POST /{id}/reserve` *(Member)* – reserve a spot
- `DELETE /reservations/{reservationId}` *(any authenticated)* – cancel a reservation
- `GET /{id}/reservations` *(Admin, Staff)* – list reservations for a class
- `DELETE /{id}` *(Admin, Staff)* – delete a class

### Payments (`/api/payments`) *(Admin, Staff)*
- `POST /` · `GET /` · `GET /{id}` · `GET /member/{memberId}` · `PUT /{id}` · `DELETE /{id}`

Sample HTTP requests are available in `GymManagement.Api/GymManagement.http`. Interactive docs are available at `/scalar/v1` when running in Development.

## Database Migrations

Migrations are applied automatically in Development. To manage them manually:

```bash
cd GymManagement.Api
dotnet ef migrations add <Name>
dotnet ef database update
```

## Project Structure

```
GymManagement/
├── GymManagement.slnx          # Solution file
├── global.json                 # Pinned .NET SDK version
├── GymManagement.Api/          # ASP.NET Core Web API
└── gymManagement-front/        # React + TypeScript (Vite) frontend
```
