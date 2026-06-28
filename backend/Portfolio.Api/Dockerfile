# ─── Build Stage ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for better layer caching
COPY Portfolio.Domain/Portfolio.Domain.csproj Portfolio.Domain/
COPY Portfolio.Application/Portfolio.Application.csproj Portfolio.Application/
COPY Portfolio.Infrastructure/Portfolio.Infrastructure.csproj Portfolio.Infrastructure/
COPY Portfolio.Api/Portfolio.Api.csproj Portfolio.Api/

# Restore dependencies
RUN dotnet restore Portfolio.Api/Portfolio.Api.csproj

# Copy all source code
COPY . .

# Publish optimized release build
RUN dotnet publish Portfolio.Api/Portfolio.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ─── Runtime Stage ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install libgdiplus for QuestPDF rendering
RUN apt-get update && apt-get install -y libgdiplus && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Render uses PORT env var
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Portfolio.Api.dll"]
