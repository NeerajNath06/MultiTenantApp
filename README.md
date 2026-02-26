# MultiTenantApp

## Production – secrets from environment

Do **not** store connection strings or JWT secrets in `appsettings.json`. For production, set these in the **environment** so no secrets live in config files:

| Variable | Required | Description |
|----------|----------|-------------|
| `ConnectionStrings__DefaultConnection` | Yes | Connection string (SQL Server or PostgreSQL) |
| `Database__Provider` | No | `SqlServer` \| `PostgreSQL` (default from appsettings) |
| `JwtSettings__SecretKey` | Yes | JWT signing key (e.g. 32+ chars) |
| `JwtSettings__Issuer` | No | JWT issuer (default: SecurityAgencyApp) |
| `JwtSettings__Audience` | No | JWT audience (default: SecurityAgencyApp) |

**Switching database (SqlServer / PostgreSQL / future MySql)**  
Set in appsettings or env: `Database:Provider` = `SqlServer` or `PostgreSQL`. Use the matching connection string. For **Render PostgreSQL**, set `ConnectionStrings__DefaultConnection` to the External Database URL (e.g. `postgresql://user:pass@host.oregon-postgres.render.com/dbname`) or key-value form: `Host=...;Port=5432;Database=securityagency_db;Username=...;Password=...`.

**Examples**

- **Windows (cmd):**  
  `set ConnectionStrings__DefaultConnection=Host=localhost;Database=SecurityGuardAppDb;Username=postgres;Password=...`  
  `set Database__Provider=PostgreSQL`  
  `set JwtSettings__SecretKey=YourProductionSecretKey...`

- **Windows (PowerShell):**  
  `$env:ConnectionStrings__DefaultConnection = "Host=..."; $env:Database__Provider = "PostgreSQL"; $env:JwtSettings__SecretKey = "..."`

- **Linux / macOS / Render:**  
  `export ConnectionStrings__DefaultConnection="postgresql://user:pass@host/dbname"; export Database__Provider=PostgreSQL`

For local development, use **User Secrets** (e.g. `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."` in the API project) or the same env vars.