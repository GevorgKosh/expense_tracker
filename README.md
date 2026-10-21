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

## Tests

```bash
dotnet test
```
