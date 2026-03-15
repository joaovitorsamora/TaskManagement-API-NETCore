# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copia a solução e o projeto para o build
COPY ../GerenciadorDeTarefas.sln ./
COPY . ./GerenciadorDeTarefas/

WORKDIR /app/GerenciadorDeTarefas
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/GerenciadorDeTarefas/out .
EXPOSE 7189
ENTRYPOINT ["dotnet", "GerenciadorDeTarefas.dll"]
