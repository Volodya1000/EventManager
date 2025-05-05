# 1) Runtime-образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5001

# 2) Сборка и публикация
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем проекты и делаем restore
COPY ["EventManager.Api/EventManager.Api.csproj", "EventManager.Api/"]
COPY ["EventManager.Application/EventManager.Application.csproj", "EventManager.Application/"]
COPY ["EventManager.Domain/EventManager.Domain.csproj", "EventManager.Domain/"]
COPY ["EventManager.Persistence/EventManager.Persistence.csproj", "EventManager.Persistence/"]
COPY ["EventManager.Infrastructure/EventManager.Infrastructure.csproj", "EventManager.Infrastructure/"]
RUN dotnet restore "EventManager.Api/EventManager.Api.csproj"

# Копируем всё и публикуем в /app/publish
COPY . .
WORKDIR "/src/EventManager.Api"
RUN dotnet publish "EventManager.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# 3) Финальный образ
FROM base AS final
WORKDIR /app

# Создаем папку для сертификатов и копируем сертификат
RUN mkdir -p /app/certificates
COPY Cert/localhost-dev.pfx /app/certificates/aspnetapp.pfx

# Копируем опубликованное приложение
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "EventManager.Api.dll"]
