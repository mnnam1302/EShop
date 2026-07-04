# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# ============================================================
# Stage 1: Base runtime - Use Alpine for smaller image size
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base

# Install wget for healthcheck support
RUN apk add --no-cache wget

USER $APP_UID
WORKDIR /app
EXPOSE 8080


# ============================================================
# Stage 2: Build - Use SDK for compilation
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]

COPY ["Order/src/EShop.Order.API/EShop.Order.API.csproj", "Order/src/EShop.Order.API/"]
COPY ["Order/src/EShop.Order.Application/EShop.Order.Application.csproj", "Order/src/EShop.Order.Application/"]
COPY ["Order/src/EShop.Order.Domain/EShop.Order.Domain.csproj", "Order/src/EShop.Order.Domain/"]
COPY ["Order/src/EShop.Order.Infrastructure/EShop.Order.Infrastructure.csproj", "Order/src/EShop.Order.Infrastructure/"]

COPY ["EShop.ServiceDefaults/EShop.ServiceDefaults.csproj", "EShop.ServiceDefaults/"]

COPY ["Shared/src/EShop.Shared.Authentication/EShop.Shared.Authentication.csproj", "Shared/src/EShop.Shared.Authentication/"]
COPY ["Shared/src/EShop.Shared.Cache/EShop.Shared.Cache.csproj", "Shared/src/EShop.Shared.Cache/"]
COPY ["Shared/src/EShop.Shared.Contracts/EShop.Shared.Contracts.csproj", "Shared/src/EShop.Shared.Contracts/"]
COPY ["Shared/src/EShop.Shared.CQRS/EShop.Shared.CQRS.csproj", "Shared/src/EShop.Shared.CQRS/"]
COPY ["Shared/src/EShop.Shared.DbResourceAccessControl/EShop.Shared.DbResourceAccessControl.csproj", "Shared/src/EShop.Shared.DbResourceAccessControl/"]
COPY ["Shared/src/EShop.Shared.Diagnostics/EShop.Shared.Diagnostics.csproj", "Shared/src/EShop.Shared.Diagnostics/"]
COPY ["Shared/src/EShop.Shared.DomainTools/EShop.Shared.DomainTools.csproj", "Shared/src/EShop.Shared.DomainTools/"]
COPY ["Shared/src/EShop.Shared.EventBus/EShop.Shared.EventBus.csproj", "Shared/src/EShop.Shared.EventBus/"]
COPY ["Shared/src/EShop.Shared.HealthChecks/EShop.Shared.HealthChecks.csproj", "Shared/src/EShop.Shared.HealthChecks/"]
COPY ["Shared/src/EShop.Shared.JsonApi/EShop.Shared.JsonApi.csproj", "Shared/src/EShop.Shared.JsonApi/"]
COPY ["Shared/src/EShop.Shared.Scoping/EShop.Shared.Scoping.csproj", "Shared/src/EShop.Shared.Scoping/"]

RUN dotnet restore "Order/src/EShop.Order.API/EShop.Order.API.csproj"

COPY . .

WORKDIR "/src/Order/src/EShop.Order.API"
RUN dotnet build "EShop.Order.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build


# ============================================================
# Stage 3: Publish - Create optimized runtime artifacts
# ============================================================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "EShop.Order.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false


# ============================================================
# Stage 4: Final runtime image
# ============================================================
FROM base AS final

LABEL org.opencontainers.image.title="EShop.Order.API" \
      org.opencontainers.image.description="Order microservice for EShop platform" \
      org.opencontainers.image.vendor="EShop" \
      org.opencontainers.image.version="1.0.0"

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "EShop.Order.API.dll"]
