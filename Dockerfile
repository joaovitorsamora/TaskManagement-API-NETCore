# =========================
# Stage 1 — Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy solution and project files first (better Docker cache)
COPY GerenciadorDeTarefas.sln ./
COPY GerenciadorDeTarefas/*.csproj ./GerenciadorDeTarefas/

# Restore dependencies
RUN dotnet restore GerenciadorDeTarefas.sln

# Copy remaining source code
COPY . .

# Publish application
RUN dotnet publish GerenciadorDeTarefas/GerenciadorDeTarefas.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# =========================
# Stage 2 — Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Install curl (used by Fly health checks)
RUN apt-get update \
    && apt-get install -y curl \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user (security best practice)
RUN useradd -m -u 1000 -s /bin/bash dotnetuser

WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

RUN chown -R dotnetuser:dotnetuser /app

USER dotnetuser

# Fly.io configuration
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "GerenciadorDeTarefas.dll"]