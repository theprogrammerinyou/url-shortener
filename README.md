# URL Shortener

A simple URL shortener built with .NET 10.0 using a clean architecture approach.

## Features & Scenarios

### Greenfield Scenarios (New Systems/Features)
- **Generate Shorter URL**: Instantly converts any long URL into a compact, shareable short code. Includes duplicate prevention where the same URL for a user yields the same short link.
- **Login and Signup**: Complete user authentication using JWT Bearer tokens to secure dashboards, manage user preferences, and assign private links.
- **Workspace Settings**: Managed profile, masking API key regeneration, and custom theme switches.

### Brownfield Scenarios (Enhancements, Refactors, & Bug Fixes)
- **Set Expiration Time**: Users can optionally define custom expiration timestamps for their links; expired codes automatically return a 404.
- **Generate QR Code**: Built-in visual QR code generator for easy offline-to-online sharing with dedicated scan count tracking.
- **Cache Data for Faster Retrieval**: Implemented the cache-aside pattern using Redis to store and resolve url mapping definitions instantly, mitigating database bottlenecks.
- **Analytics with Click Count**: Full-featured analytics dashboard documenting click counts today, engagement growth over time, geolocation maps, and top referral sources.
- **Privacy Controls**: Custom aliases validation and private links toggling to hide links from public directories.
- **Refactoring to Low-Level Design (LLD)**: Restructured core models (e.g. `UrlEntry` replacing `UrlMapping`), abstractions (e.g. `IUrlRepository` and `IKeyGenerator`), and repositories mapping to Neon serverless PostgreSQL.
- **Secure Guest Access**: Secured data retrieval ensuring a user's local IP-based unique guest fingerprint restricts access so they cannot see another user's links.
- **Client IP-Partitioned Rate Limiting**: Added ASP.NET Core rate limiting middleware to control API request volumes. Write traffic (URL creation) uses a client IP-partitioned Token Bucket policy (10 tokens max, refills 5/minute). Read traffic (URL redirection) uses a client IP-partitioned Sliding Window Counter policy (100 requests/minute split across 6 segments).

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
