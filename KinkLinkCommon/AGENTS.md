# KinkLinkCommon Agent Guide

## Project Overview
Shared library (net8.0) containing domain models, network contracts, utilities, and database access code used across all KinkLink projects.

## Build Commands
```bash
# Build project
dotnet build KinkLinkCommon.csproj

# Note: This is a class library - no executable output
```

## Key Dependencies
- ASP.NET Core SignalR MessagePack protocol
- Npgsql for PostgreSQL connectivity
- Serilog for logging
- Microsoft.Extensions.Logging

## Architecture Patterns
- Domain-driven design with clear separation of concerns
- MessagePack serialization for network contracts
- Extension methods for enum utilities
- Abstracted database access with SQL classes
- Security interfaces and implementations

## Code Style
- File-scoped namespaces
- `record` types for immutable data structures
- MessagePack attributes on network contracts
- Extension methods in separate files
- Enum classes for type safety

## Key Directories
- `Domain/Network/` - Network message contracts with MessagePack
- `Domain/Enums/` - Type-safe enums and constants
- `Security/` - Hashing and security interfaces
- `Database/` - SQL query classes and database access
- `Util/` - Extension methods and utilities
- `Dependencies/` - External plugin domain models

## Network Contracts
- All network types use MessagePack serialization
- Request/Response pattern with generic ActionResult
- Separate files for each operation type
- Key attributes for property ordering

## Database Access
- SQL query classes for each domain entity
- Parameterized queries for security
- Connection handling through DatabaseService

## Extensions
- Enum extension methods for flags and utilities
- Character attribute helpers
- Chat channel extensions

This library serves as the foundation for data consistency across all KinkLink components.