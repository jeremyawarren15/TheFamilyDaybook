# The Family Daybook

A Blazor Web Application built with .NET 9.

## Project Structure

This solution follows a clean architecture pattern with separation of concerns:

```
TheFamilyDaybook/
├── src/
│   ├── TheFamilyDaybook.Web/          # Blazor Web Application (UI Layer)
│   ├── TheFamilyDaybook.Models/       # Domain Models and Entities
│   └── TheFamilyDaybook.Contracts/    # Interfaces, DTOs, and Contracts
└── TheFamilyDaybook.sln               # Solution file
```

### Projects

#### TheFamilyDaybook.Web
- **Purpose**: The main Blazor Web Application
- **Type**: Blazor Web App (supports both Server and WebAssembly)
- **References**: 
  - TheFamilyDaybook.Models
  - TheFamilyDaybook.Contracts
- **Contains**: 
  - Blazor components and pages
  - UI logic
  - Program.cs (startup configuration)

#### TheFamilyDaybook.Models
- **Purpose**: Domain models, entities, and data structures
- **Type**: Class Library
- **References**: None (base layer)
- **Contains**: 
  - Domain entities
  - Data models
  - Value objects

#### TheFamilyDaybook.Contracts
- **Purpose**: Interfaces, DTOs (Data Transfer Objects), and service contracts
- **Type**: Class Library
- **References**: 
  - TheFamilyDaybook.Models
- **Contains**: 
  - Service interfaces
  - Repository interfaces
  - DTOs for API communication
  - Contracts and abstractions

## Getting Started

### Prerequisites
- .NET 9 SDK installed

### Build and Run

```bash
# Restore packages
dotnet restore

# Build the solution
dotnet build

# Run the web application
cd src/TheFamilyDaybook.Web
dotnet run
```

Or from the solution root:
```bash
dotnet run --project src/TheFamilyDaybook.Web/TheFamilyDaybook.Web.csproj
```

The application will be available at:
- HTTPS: https://localhost:5001
- HTTP: http://localhost:5000

## Adding New Projects

To add a new project to the solution:

```bash
# Create a new class library
dotnet new classlib -n TheFamilyDaybook.Services -o src/TheFamilyDaybook.Services

# Add to solution
dotnet sln add src/TheFamilyDaybook.Services/TheFamilyDaybook.Services.csproj

# Add reference from another project
dotnet add src/TheFamilyDaybook.Web/TheFamilyDaybook.Web.csproj reference src/TheFamilyDaybook.Services/TheFamilyDaybook.Services.csproj
```

## Common Project Types

- **Class Library** (`dotnet new classlib`) - For shared code, services, data access
- **xUnit Test** (`dotnet new xunit`) - For unit tests
- **Web API** (`dotnet new webapi`) - For REST APIs (if needed separately)

## Architecture Notes

This structure allows for:
- **Separation of Concerns**: Each project has a clear responsibility
- **Dependency Management**: Dependencies flow in one direction (Web → Contracts → Models)
- **Testability**: Interfaces in Contracts make it easy to mock dependencies
- **Scalability**: Easy to add new projects (Services, Data, Tests) as needed



