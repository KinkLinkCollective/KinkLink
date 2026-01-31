# Use the official .NET 9.0 runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5006
EXPOSE 9090

# Use the .NET 9.0 SDK as build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["KinkLinkServer/KinkLinkServer.csproj", "KinkLinkServer/"]
COPY ["KinkLinkCommon/KinkLinkCommon.csproj", "KinkLinkCommon/"]
RUN dotnet restore "KinkLinkServer/KinkLinkServer.csproj"

# Copy the entire source code
COPY . .

# Build the application
WORKDIR "/src/KinkLinkServer"
RUN dotnet build "KinkLinkServer.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "KinkLinkServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Create the final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directory for configuration
RUN mkdir -p /app/Configuration

# Set environment variables for containerized deployment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=https://+:5006

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f https://localhost:5006/health || exit 1

ENTRYPOINT ["dotnet", "KinkLinkServer.dll"]
