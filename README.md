# MultiTenantApp

## Production – secrets from environment

Do **not** store connection strings or JWT secrets in `appsettings.json`. For production, set these in the **environment** so no secrets live in config files:

| Variable | Required | Description |
|----------|----------|-------------|
| `ConnectionStrings__DefaultConnection` | Yes | SQL Server connection string |
| `JwtSettings__SecretKey` | Yes | JWT signing key (e.g. 32+ chars) |
| `JwtSettings__Issuer` | No | JWT issuer (default: SecurityAgencyApp) |
| `JwtSettings__Audience` | No | JWT audience (default: SecurityAgencyApp) |

**Examples**

- **Windows (cmd):**  
  `set ConnectionStrings__DefaultConnection=Server=...;Database=...;User ID=...;Password=...`  
  `set JwtSettings__SecretKey=YourProductionSecretKey...`

- **Windows (PowerShell):**  
  `$env:ConnectionStrings__DefaultConnection = "Server=..."; $env:JwtSettings__SecretKey = "..."`

- **Linux / macOS / containers:**  
  `export ConnectionStrings__DefaultConnection="Server=..."; export JwtSettings__SecretKey="..."`

For local development, use **User Secrets** (e.g. `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."` in the API project) or the same env vars.