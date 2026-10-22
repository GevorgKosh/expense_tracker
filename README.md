# Personal Expense Tracker API

ASP.NET Core Web API for tracking personal expenses by category, using EF Core with SQL Server.

## Database setup

### Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB, Express or full)
- EF Core CLI tools:

  ```bash
  dotnet tool install --global dotnet-ef
  # or, if already installed
  dotnet tool update --global dotnet-ef
  ```

### JWT signing key (.env)

The JWT signing key is a secret and is not stored in the repository. It is read from a `.env` file in the solution root, which is in `.gitignore`:

```bash
cp .env.example .env
openssl rand -base64 64   # paste the output into .env, inside the single quotes
```

```
AppSettings__Token='<your-generated-key>'
```

On startup the values in `.env` are loaded as environment variables and override `appsettings.json` (`AppSettings__Token` → `AppSettings:Token`). Environment variables that are already set (e.g. in Docker) take priority over `.env`. The API refuses to start if the key is missing.

### Connection string

Set `ConnectionStrings:UserDatabase` in `ExpenseTracker/appsettings.json` (or `appsettings.Development.json`) to point at your SQL Server instance:

```json
"ConnectionStrings": {
  "UserDatabase": "Server=localhost\\SQLEXPRESS;Database=UserDb;Trusted_Connection=true;TrustServerCertificate=true;"
}
```

### Apply migrations

From the solution root:

```bash
dotnet ef database update --project ExpenseTracker
```

This creates the database (if it does not exist), applies all migrations in `ExpenseTracker/Migrations`, and seeds the default categories.

When running in the `Development` environment, pending migrations are also applied automatically on startup (`Database.Migrate()` in `Program.cs`).

### Adding a new migration

After changing entities or `ExpenseTrackerDbContext`:

```bash
dotnet ef migrations add <MigrationName> --project ExpenseTracker
dotnet ef database update --project ExpenseTracker
```

To undo the last migration that has not been applied yet:

```bash
dotnet ef migrations remove --project ExpenseTracker
```

## Running

```bash
dotnet run --project ExpenseTracker
```

Swagger UI is available in `Development` at `/swagger`.

## Running with Docker Compose

Runs the API and SQL Server 2022 in containers; no local SQL Server or .NET SDK needed. Requires Docker.

1. Create `.env` from `.env.example` and set both `AppSettings__Token` and `MSSQL_SA_PASSWORD`.
2. Start everything:

   ```bash
   docker compose up --build
   ```

The API waits until SQL Server is healthy, then applies the migrations on startup.

- Swagger UI: http://localhost:8080/swagger
- Health check: http://localhost:8080/health
- SQL Server (e.g. for SSMS): `localhost,14330`, user `sa`, password from `.env`

The database is kept in the `sqlserver-data` volume between runs. `docker compose down -v` removes it.

## Tests

```bash
dotnet test
```
