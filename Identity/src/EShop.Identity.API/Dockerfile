# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app

USER root
EXPOSE 2000

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Identity/src/EShop.Identity.API/EShop.Identity.API.csproj", "Identity/src/EShop.Identity.API/"]
COPY ["Shared/src/EShop.Shared.Contracts/EShop.Shared.Contracts.csproj", "Shared/src/EShop.Shared.Contracts/"]
COPY ["Shared/src/EShop.Shared.Diagnostics/EShop.Shared.Diagnostics.csproj", "Shared/src/EShop.Shared.Diagnostics/"]
COPY ["Shared/src/EShop.Shared.JsonApi/EShop.Shared.JsonApi.csproj", "Shared/src/EShop.Shared.JsonApi/"]
COPY ["Shared/src/EShop.Shared.Cache/EShop.Shared.Cache.csproj", "Shared/src/EShop.Shared.Cache/"]
COPY ["Shared/src/EShop.Shared.Scoping/EShop.Shared.Scoping.csproj", "Shared/src/EShop.Shared.Scoping/"]
COPY ["Shared/src/EShop.Shared.DomainTools/EShop.Shared.DomainTools.csproj", "Shared/src/EShop.Shared.DomainTools/"]
COPY ["Shared/src/EShop.Shared.DbResourceAccessControl/EShop.Shared.DbResourceAccessControl.csproj", "Shared/src/EShop.Shared.DbResourceAccessControl/"]
COPY ["Identity/src/EShop.Identity.Application/EShop.Identity.Application.csproj", "Identity/src/EShop.Identity.Application/"]
COPY ["Identity/src/EShop.Identity.Domain/EShop.Identity.Domain.csproj", "Identity/src/EShop.Identity.Domain/"]
COPY ["Identity/src/EShop.Identity.Infrastructure/EShop.Identity.Infrastructure.csproj", "Identity/src/EShop.Identity.Infrastructure/"]
COPY ["Identity/src/EShop.Identity.Persistence/EShop.Identity.Persistence.csproj", "Identity/src/EShop.Identity.Persistence/"]
COPY ["Identity/src/EShop.Identity.Presentation/EShop.Identity.Presentation.csproj", "Identity/src/EShop.Identity.Presentation/"]

RUN dotnet restore "./Identity/src/EShop.Identity.API/EShop.Identity.API.csproj"

COPY . .

WORKDIR "/src/Identity/src/EShop.Identity.API"
RUN dotnet build "./EShop.Identity.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EShop.Identity.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EShop.Identity.API.dll"]