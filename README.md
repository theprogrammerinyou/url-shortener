# URL Shortener

A simple URL shortener built with .NET 10.0 using a clean architecture approach.

## Features
- Shorten long URLs
- Redirect short codes to original URLs
- In-memory storage for rapid prototyping
- Swagger/OpenAPI documentation
- Unit tests for core service and repository functionality

## Project Structure
- `Client` - contains the react code combined with MUI and React Router
- `UrlShortener.Api` - ASP.NET Core Web API host
- `UrlShortener.Core` - domain models, service interfaces, DTOs, and business logic
- `UrlShortener.Infrastructure` - concrete repository and short-code generator implementations
- `UrlShortener.Tests` - xUnit unit tests

## Running the application

### 1. Run the Backend API (via Docker)
Prerequisite: Make sure you have Docker and Docker Compose installed and running.
Ensure the root `.env` contains your PostgreSQL database credentials and Redis configurations.

Run the API and local Redis instance:
```bash
docker compose up --build
```

Access Swagger UI:
- **Local Development (Docker)**: `http://localhost:5001/swagger/index.html`
- **Production (Render)**: `https://linkswift-api.onrender.com/swagger/index.html`

### 2. Run the Frontend (React SPA)
Run the React development server:
```bash
cd client
npm install
npm start
```
This will launch the web application in your browser at `http://localhost:3000`.

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
