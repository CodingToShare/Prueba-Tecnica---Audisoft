# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /src

# Copy the entire project structure
COPY . .

# Restore dependencies - only for the API project to avoid test project issues
RUN dotnet restore "src/AudiSoft.School.Api/AudiSoft.School.Api.csproj"

# Build the API project
RUN dotnet build "src/AudiSoft.School.Api/AudiSoft.School.Api.csproj" -c Release -o /app/build

# Publish
RUN dotnet publish "src/AudiSoft.School.Api/AudiSoft.School.Api.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Install SQL Server tools (optional, for migrations)
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copy published application from builder
COPY --from=builder /app/publish .

# Copy database initialization scripts
COPY ["scripts/", "/app/scripts/"]

# Expose API port
EXPOSE 5281

# Environment variables defaults
ENV ASPNETCORE_URLS=http://+:5281
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__DefaultConnection=""

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:5281/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "AudiSoft.School.Api.dll"]
