# Architecture Overview

## Layers

### `UrlShortener.Api`
- Hosts the ASP.NET Core Web API
- Defines controllers and request routing
- Composes application services using dependency injection
- Exposes Swagger/OpenAPI documentation

### `UrlShortener.Core`
- Defines domain entities and DTOs
- Declares abstraction contracts for repository and short-code generator
- Contains business logic in `UrlShortenerService`

### `UrlShortener.Infrastructure`
- Implements repository and short-code generation logic
- Provides `InMemoryUrlMappingRepository` for data storage
- Provides `Base62ShortCodeGenerator` for generating unique codes

### `UrlShortener.Tests`
- Includes unit tests for core service and infrastructure behavior

## Component Responsibilities

### `IUrlMappingRepository`
- Abstraction for CRUD operations on URL mappings
- Keeps storage details decoupled from business logic

### `IShortCodeGenerator`
- Abstraction for generating short codes
- Allows swapping different generation strategies

### `IUrlShortenerService`
- Application service interface for URL shortening workflows
- Handles creation, resolution, and retrieval of URL mappings

### `UrlShortenerService`
- Encapsulates business rules
- Normalizes original URLs
- Reuses existing short codes for duplicate URLs
- Increments click counts on resolves

### `InMemoryUrlMappingRepository`
- Provides thread-safe in-memory storage
- Uses `ConcurrentDictionary` for fast lookups

### `Base62ShortCodeGenerator`
- Generates nondeterministic short codes using Base62 characters
- Uses cryptographic RNG for token creation

## Sequence
1. Client sends POST `/api/UrlShortener` with original URL
2. `UrlShortenerController` forwards request to `IUrlShortenerService`
3. `UrlShortenerService` checks for duplicates and generates a short code
4. Mapping is stored in `InMemoryUrlMappingRepository`
5. Client can GET `/{shortCode}` and `RedirectController` resolves and redirects

## Extensibility
- Swap the repository implementation to persist data to a database
- Add additional validation or analytics without changing controller logic
- Add caching or rate limiting in a decorator around `IUrlShortenerService`
