# Employee Management System
repo:
https://github.com/taloshiliN/EmployeeManagementSystem
users:
johndoe@example.com
Password123!

An ASP.NET Core 9 template that pairs a server-rendered MVC admin UI with a token-secured REST API over the same domain services. Built as a reusable starting point for CRUD-style line-of-business apps.

## Stack

| Concern | Choice |
|---|---|
| Framework | ASP.NET Core 9.0 (MVC + Web API in one host) |
| Data | Entity Framework Core 9 + SQL Server |
| Identity | ASP.NET Core Identity (`ApplicationUser : IdentityUser`) |
| Web auth | Cookie (`Identity.Application`) |
| API auth | JWT bearer, role claims |
| API docs | Swashbuckle / Swagger UI |
| UI | Razor views + Bootstrap 5 |

## What's in it

- **Three domain entities** — `Employee`, `Department`, `JobTitle` — each with a service, an MVC controller and an API controller.
- **Dual authentication.** The browser UI uses cookies; the API uses JWTs. Both are registered side by side, so a single deployment serves an admin site and a machine-facing API.
- **Role-based authorisation** on the employee API (`Admin`, `HR`, `Employee`).
- **A service layer** that owns all validation and business rules, so the MVC and API controllers stay thin and never duplicate logic.
- **Centralised exception handling** that maps exception types to HTTP status codes.
- **Filtering, paging, and a raw-ADO.NET stored procedure call** as worked examples of three different data-access styles.

## Project structure

```
Controllers/          MVC controllers (Departments, Employees, Account, Home)
                      + API controllers (*ApiController, AuthController)
Data/                 ApplicationDbContext
DTOs/                 API request/response shapes
Middleware/           ExceptionHandlingMiddleware
Migrations/           EF Core migrations
Models/               EF entities + ApplicationUser
Services/             Business logic — I*Service + implementation per entity
ViewModels/           Form and page models for Razor views
Views/                Razor views, grouped by controller
```

The layering rule: **controllers never touch `DbContext`.** They call a service. Services own validation, uniqueness checks and referential rules, and throw typed exceptions on failure.

## Getting started

### Prerequisites

- .NET 9 SDK
- SQL Server (LocalDB, Express, or full)
- `dotnet-ef` tools: `dotnet tool install --global dotnet-ef`

### 1. Configure the connection string

`appsettings.json` ships with a machine-specific server name. Change it, or better, keep it out of source control entirely:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=.\SQLEXPRESS;database=employee_management_system;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:Key" "<a long random string, 32+ characters>"
```

### 2. Create the database

```bash
dotnet ef database update
```

### 3. Create the stored procedure

One endpoint (`GET /api/employees/by-department-sp/{id}`) calls a stored procedure that **is not created by migrations**. Run this against the database once, or that endpoint will fail:

```sql
CREATE OR ALTER PROCEDURE GetEmployeesByDepartment
    @DepartmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.Id,
        e.FirstName,
        e.LastName,
        e.Email,
        e.Position,
        d.DepartmentName,
        j.Title AS JobTitle,
        e.Salary
    FROM Employees e
    INNER JOIN Departments d ON d.Id = e.DepartmentId
    INNER JOIN JobTitles  j ON j.Id = e.JobTitleId
    WHERE e.DepartmentId = @DepartmentId;
END
```

### 4. Run

```bash
dotnet watch
```

| URL | What |
|---|---|
| `https://localhost:7266` | MVC UI |
| `http://localhost:5273` | MVC UI (http profile) |
| `https://localhost:7266/swagger` | Swagger UI (Development only) |

### 5. Create your first user

There is no seeded account and no browser registration page. Register through the API:

```bash
curl -X POST https://localhost:7266/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Admin User","email":"admin@example.com","password":"Passw0rd!","role":"Admin"}'
```

Then sign in at `/Account/Login` with that email and password.

## Authentication

Two schemes coexist, and this is the part most likely to trip you up:

- **The default scheme is JWT.** `Program.cs` sets `DefaultAuthenticateScheme = JwtBearer`, so anything that doesn't say otherwise expects a bearer token.
- **MVC controllers must opt into cookies explicitly:**

  ```csharp
  [Authorize(AuthenticationSchemes = "Identity.Application")]
  ```

  Omit that and your Razor pages will silently demand a JWT and redirect nowhere useful.
- **API controllers opt into JWT explicitly:**

  ```csharp
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  ```

Get a token from `POST /api/auth/login`; the response includes the token, email, full name and roles. In Swagger, click **Authorize** and paste the raw token.

## API reference

All routes are relative to the host root.

### Auth

| Method | Route | Auth | Notes |
|---|---|---|---|
| POST | `/api/auth/register` | anonymous | Roles: `Admin`, `HR`, `Employee` |
| POST | `/api/auth/login` | anonymous | Returns JWT + roles |

### Employees — `/api/employees`

| Method | Route | Roles |
|---|---|---|
| GET | `/api/employees` | any authenticated |
| GET | `/api/employees/{id}` | any authenticated |
| GET | `/api/employees/filter` | any authenticated |
| GET | `/api/employees/paged` | any authenticated |
| GET | `/api/employees/by-department-sp/{departmentId}` | any authenticated |
| POST | `/api/employees` | `Admin`, `HR` |
| PUT | `/api/employees/{id}` | `Admin`, `HR` |
| DELETE | `/api/employees/{id}` | `Admin` |

`filter` accepts `keyword`, `departmentId`, `jobTitleId`, `minSalary`, `maxSalary`. `paged` accepts `pageNumber` and `pageSize`.

### Departments — `/api/department`

| Method | Route |
|---|---|
| GET | `/api/department` |
| GET | `/api/department/{id}` |
| POST | `/api/department` |
| PUT | `/api/department/{id}` |
| DELETE | `/api/department/{id}` |

### Job titles — `/api/jobtitle`

Same five-verb shape as departments.

## Web routes

Default route pattern is `{controller=Home}/{action=Index}/{id?}`.

| Route | Purpose |
|---|---|
| `/Employees` | List, with Details / Edit / Delete |
| `/Employees/Create`, `/Employees/Edit/{id}` | Forms with department + job-title dropdowns |
| `/Departments` | List with employee counts, keyword search, paging |
| `/Departments/Details/{id}` | Department plus its employee roster |
| `/Departments/Create`, `/Departments/Edit/{id}` | Forms |
| `/Account/Login`, `/Account/Logout` | Cookie sign-in |

> **Controller name vs route token.** The classes are `EmployeesController` and `DepartmentsController`, so the route tokens are `Employees` and `Departments` — **plural**. Tag helpers written as `asp-controller="Department"` will generate a URL that 404s at runtime without any build error. If a form submits into the void, check this first.

## Error handling

`ExceptionHandlingMiddleware` sits at the very top of the pipeline and maps exceptions to status codes:

| Exception | Status |
|---|---|
| `ArgumentException` | 400 Bad Request |
| `InvalidOperationException` | 409 Conflict |
| `KeyNotFoundException` | 404 Not Found |
| `UnauthorizedAccessException` | 403 Forbidden |
| anything else | 500, message replaced with a generic string |

It always writes **JSON**. That is right for the API and wrong for the MVC UI — a user who trips a rule in the browser sees a raw JSON blob instead of a validation message on the form. So MVC controllers must catch service exceptions themselves and push them into `ModelState`:

```csharp
catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
{
    ModelState.AddModelError(string.Empty, ex.Message);
    return View(viewModel);
}
```

Both exception types matter: services throw `ArgumentException` for invalid input and `InvalidOperationException` for conflicts (duplicate names, deleting a department that still has employees). Catching only one leaves a silent hole.

## Adding a new entity

The three existing entities follow an identical shape. To add a fourth:

1. **Model** in `Models/` with `Id` and any navigation collections.
2. **`DbSet<T>`** on `ApplicationDbContext`, then `dotnet ef migrations add Add<Entity>` and `dotnet ef database update`.
3. **`I<Entity>Service` + `<Entity>Service`** in `Services/` — all validation lives here, throwing `ArgumentException` (bad input) or `InvalidOperationException` (conflict).
4. **Register it** in `Program.cs`: `builder.Services.AddScoped<I<Entity>Service, <Entity>Service>();`
5. **DTOs** in `DTOs/` — `Create*`, `Update*`, `*ResponseDto`.
6. **View models** in `ViewModels/` — a shared `<Entity>FormViewModel` base with `Create*` and `Edit*` deriving from it. `Edit*` adds `Id`.
7. **API controller** — `[ApiController]`, `[Route("api/<entity>")]`, JWT-authorised.
8. **MVC controller** — plural class name, `[Authorize(AuthenticationSchemes = "Identity.Application")]`.
9. **Views** in `Views/<Plural>/` — Index, Create, Edit, Delete, Details. Copy an existing set and be careful to update every `asp-controller` value.
10. **Nav link** in `Views/Shared/_Layout.cshtml`.

### Two Razor traps

- `page`, `model`, `using`, `inject`, `section` and `functions` are reserved `@`-directives. Never use them as local variable names in a `.cshtml` — `@page` inside a loop is a parse error, not a variable.
- Keep `<th>` and `<td>` counts aligned when you add an actions column.

## Known limitations

- **Registration is unauthenticated and accepts any role**, including `Admin`. Fine for local development; lock this down before anything real.
- **The department and job-title APIs have no `[Authorize]` attribute** — they are wide open, unlike the employee API.
- `appsettings.json` contains a real connection string and a JWT signing key. Move both to user secrets or environment variables.
- No unit or integration tests.
- No seed data — the database starts empty, so create a department and a job title before creating an employee.
- Job titles have API endpoints but no MVC UI.
- HTTPS redirection is on, so a `curl` against `http://localhost:5273/api/...` will redirect; use the HTTPS port.

## Conventions worth keeping

- Services return `bool` for update/delete (`false` = not found) and throw for rule violations. Controllers translate that into `NotFound()` or a `ModelState` error.
- List endpoints project into DTOs rather than loading navigation collections, so counts become SQL `COUNT(*)` subqueries instead of loading every child row.
- `Include()` only where the data is actually rendered — `GetDepartmentByIdAsync` stays lean and `GetDepartmentWithEmployeesAsync` exists separately for the pages that need the roster.
