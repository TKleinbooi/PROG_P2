# PROG_P2
# RaceDayAPI

## 1. Project Overview

RaceDayAPI is a web API developed using ASP.NET Core and .NET 8. It supports race-day management by providing API functionality for managing events and participant enrolments. The project uses Entity Framework Core for database operations and includes automated tests to check that the application works correctly.

## 2. Main Features

- Manage race-day events.
- Manage participant enrolments.
- Use controllers and DTOs to handle API requests and responses.
- Connect to a database using Entity Framework Core.
- Support authentication and role-based access where configured.
- Run automated tests using xUnit.
- Use GitHub Actions to build and test the project automatically.

## 3. User Roles

### Administrator

The Administrator is responsible for managing the system and its main information. Depending on the permissions configured in the application, the Administrator can manage race events, maintain event information, and oversee participant enrolments.

### Participant

The Participant is a user who takes part in race events. Depending on the permissions configured in the application, the Participant can view available events and enrol in events.

**Note:** These descriptions should match the actual permissions implemented in the project. Update them if the API assigns different permissions to these roles.

## 4. Requirements

Before setting up the project, install the following:

- .NET 8 SDK
- Visual Studio 2022 or another editor that supports .NET 8
- A compatible database server, configured for the application
- Git, if you want to clone the repository

## 5. Setup Instructions

1. Download or clone the repository from GitHub.
2. Open the solution file, `RaceDayAPI.sln`, in Visual Studio 2022.
3. Restore the NuGet packages. In Visual Studio, right-click the solution and select **Restore NuGet Packages**, or run:

   ```bash
   dotnet restore RaceDayAPI.sln
   ```

4. Open `appsettings.json` and check the database connection string.
5. Update the connection string to match your database server and database credentials. Do not publish real passwords or secrets to GitHub.
6. Open the Package Manager Console in Visual Studio if database migrations need to be applied. Select the API project as the default project and run:

   ```powershell
   Update-Database
   ```

   Alternatively, if the EF Core command-line tools are installed and configured, use:

   ```bash
   dotnet ef database update
   ```

7. Build the solution to check that the project compiles successfully.

   ```bash
   dotnet build RaceDayAPI.sln
   ```

## 6. How to Run the API

### Using Visual Studio 2022

1. Open `RaceDayAPI.sln`.
2. Set `RaceDayAPI` as the startup project.
3. Check that the database is running and that the connection string is correct.
4. Press **F5** to run with debugging, or **Ctrl + F5** to run without debugging.
5. If Swagger is enabled in the current environment, use the Swagger page to view and test the available endpoints.

### Using the Command Line

From the directory containing `RaceDayAPI.csproj`, run:

```bash
dotnet run --project RaceDayAPI.csproj
```

The terminal will display the address where the API is running. Open that address in a browser or API testing tool. Add `/swagger` if Swagger is enabled.

## 7. Running the Automated Tests

The project includes an automated test project named `RaceDay.Test.csproj`.

From the directory containing the solution, run:

```bash
dotnet test RaceDayAPI.sln
```

The test command builds the projects and runs the available automated tests. A successful run indicates that the tests passed.

## 8. Project Structure

- `Controllers/` — Handles incoming API requests.
- `DTOs/` — Contains data transfer objects used to transfer data between the client and API.
- `Data/` — Contains database-related code and configuration.
- `Migrations/` — Contains Entity Framework Core database migrations.
- `Models/` — Contains the application's data models.
- `Properties/` — Contains project launch settings.
- `Program.cs` — Configures services and starts the API.
- `RaceDayAPI.csproj` — Defines the API project and its dependencies.
- `RaceDay.Test.csproj` — Defines the automated test project.
- `RaceDayApiFactory.cs` — Provides a test application factory.
- `UnitTest1.cs` — Contains automated tests.
- `.github/workflows/dotnet.yml` — Defines the GitHub Actions workflow for building and testing the solution.

## 9. Continuous Integration

GitHub Actions is configured to build and test the solution automatically when the configured workflow is triggered. This helps identify build errors and test failures during development.

## 10. Troubleshooting

- **Database connection error:** Check that the database server is running and that the connection string is correct.
- **Package restore error:** Run `dotnet restore RaceDayAPI.sln` and check that the required NuGet packages are available.
- **Build error:** Run `dotnet build RaceDayAPI.sln` and review the error messages.
- **Test failure:** Run `dotnet test RaceDayAPI.sln` and inspect the failed test details.
- **Swagger unavailable:** Check the launch profile and confirm whether Swagger is enabled for the current environment.

## 11. Author

Developed as part of a programming project using ASP.NET Core, .NET 8, Entity Framework Core, and automated testing.

## 12. Workflow
<img width="847" height="177" alt="image" src="https://github.com/user-attachments/assets/2af6da0b-08a0-4d0e-91c1-944e45109228" />

## 13. Youtube Link
https://youtu.be/FpoBQ21fKow
