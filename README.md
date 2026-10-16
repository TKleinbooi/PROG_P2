# PROG_P2

# RaceDayAPI

## Project Overview
RaceDayAPI is a .NET 8 Web API developed to manage race-day information and related operations. The project provides API endpoints for managing data and allows participants to interact with race-related functionality.

## Technologies Used
- C#
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL database integration
- xUnit for automated testing
- GitHub for version control
- GitHub Actions for continuous integration (CI)

## Main Features
- REST API endpoints using controllers.
- Data management through models and Data Transfer Objects (DTOs).
- Database access using Entity Framework Core.
- Authentication functionality.
- Participant enrolment functionality.
- Automated tests for API functionality.
- Automated build and test workflow through GitHub Actions.

## Project Structure
- `Controllers/` – API controllers and endpoints.
- `Data/` – Database context and data access configuration.
- `Models/` – Application data models.
- `DTOs/` – Data Transfer Objects used for API requests and responses.
- `Migrations/` – Entity Framework Core database migrations.
- `Properties/` – Project configuration files.
- `RaceDayAPI.csproj` – Main API project file.
- `RaceDay.Test.csproj` – Automated test project file.
- `RaceDayAPI.sln` – Solution file.
- `.github/workflows/dotnet.yml` – GitHub Actions workflow.

## Running the Project
1. Install the .NET 8 SDK.
2. Clone or download this repository.
3. Open `RaceDayAPI.sln` in Visual Studio 2022.
4. Restore the NuGet packages.
5. Configure the database connection string in the appropriate settings file.
6. Apply the required Entity Framework Core migrations if necessary.
7. Run the API project.

## Running the Tests
Run the following commands from the repository directory:

```bash
dotnet restore RaceDayAPI.sln
dotnet build RaceDayAPI.sln --configuration Release
dotnet test RaceDayAPI.sln --configuration Release
```

## Continuous Integration
GitHub Actions automatically builds the solution and runs the automated tests when changes are pushed to the configured branches or a pull request is opened.

## Purpose
This project demonstrates API development, database integration, authentication, automated testing, version control, and continuous integration using the .NET ecosystem.
Workflow
<img width="847" height="177" alt="image" src="https://github.com/user-attachments/assets/2af6da0b-08a0-4d0e-91c1-944e45109228" />
