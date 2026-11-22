# ZeeAcom

Accommodation listings for Zeerust. Landlords register and list their properties; the admin manages owners and listings.

## Running locally

1. Apply the database migrations (SQL Server LocalDB, see `ConnectionStrings:DefaultConnection` in `appsettings.json`):

   ```
   dotnet ef database update
   ```

2. Set the admin login in user-secrets (kept outside the repo). The account is created on startup if it doesn't exist:

   ```
   dotnet user-secrets set "AdminUser:Email" "you@example.com"
   dotnet user-secrets set "AdminUser:Password" "<a strong password>"
   ```

   On a server, set the same values as environment variables (`AdminUser__Email`, `AdminUser__Password`).

3. Run the app:

   ```
   dotnet run --launch-profile http
   ```

   - Admin pages: `/Owners`, `/Properties` (Admin role)
   - Landlords: `/Account/Register`, then `/Landlord`
   - API: `/api/...` (Admin role), Swagger at `/swagger`
