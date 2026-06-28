# ─── Frontend Build Stage ───────────────────────────────────
FROM node:20-alpine AS frontend-build
WORKDIR /app/frontend

# Copy frontend dependency files
COPY frontend/package*.json ./
RUN npm ci

# Copy frontend source code and build
COPY frontend/ ./
RUN npm run build

# ─── Backend Build Stage ────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src

# Copy project files first for better layer caching
COPY backend/Portfolio.Domain/Portfolio.Domain.csproj backend/Portfolio.Domain/
COPY backend/Portfolio.Application/Portfolio.Application.csproj backend/Portfolio.Application/
COPY backend/Portfolio.Infrastructure/Portfolio.Infrastructure.csproj backend/Portfolio.Infrastructure/
COPY backend/Portfolio.Api/Portfolio.Api.csproj backend/Portfolio.Api/

# Restore dependencies
RUN dotnet restore backend/Portfolio.Api/Portfolio.Api.csproj

# Copy all backend source code
COPY backend/ ./backend/

# Publish optimized release build
RUN dotnet publish backend/Portfolio.Api/Portfolio.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ─── Runtime Stage ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install libgdiplus for QuestPDF rendering (matching previous backend Dockerfile)
RUN apt-get update && apt-get install -y libgdiplus && rm -rf /var/lib/apt/lists/*

# Copy backend publish output
COPY --from=backend-build /app/publish .

# Copy frontend build output into wwwroot
COPY --from=frontend-build /app/frontend/dist ./wwwroot

# Render uses PORT env var
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Portfolio.Api.dll"]
