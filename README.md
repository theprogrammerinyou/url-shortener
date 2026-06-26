# URL Shortener

A simple URL shortener built with .NET 10.0 using a clean architecture approach.

## Features
- Shorten long URLs
- Redirect short codes to original URLs
- In-memory storage for rapid prototyping
- Swagger/OpenAPI documentation
- Unit tests for core service and repository functionality

## Project Structure
- `UrlShortener.Api` - ASP.NET Core Web API host
- `UrlShortener.Core` - domain models, service interfaces, DTOs, and business logic
- `UrlShortener.Infrastructure` - concrete repository and short-code generator implementations
- `UrlShortener.Tests` - xUnit unit tests

## Running the application
1. Build the solution:
   ```bash
   dotnet build
   ```
2. Run the Web API:
   ```bash
   dotnet run --project UrlShortener.Api/UrlShortener.Api.csproj
   ```
3. Open Swagger UI:
   ```
   https://localhost:5001/swagger/index.html
   ```

## Running tests
```bash
cd UrlShortener.Tests
dotnet test
```

## Design Principles
- Single Responsibility: separate projects for API, domain, and infrastructure
- Dependency Inversion: API depends on abstractions from Core, not concrete implementations
- Open/Closed: new repository or generator implementations can be added without modifying core services
- In-Memory repository used for initial implementation; swapable for persistent storage later
