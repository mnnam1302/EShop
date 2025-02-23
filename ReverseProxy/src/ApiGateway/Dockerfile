# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app

USER root
EXPOSE 5000


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["EShop/Directory.Packages.props", "EShop/"]
COPY ["EShop/ReverseProxy/src/ApiGateway/ApiGateway.csproj", "EShop/ReverseProxy/src/ApiGateway/"]
COPY ["EShop/Shared/src/EShop.Shared.Cache/EShop.Shared.Cache.csproj", "EShop/Shared/src/EShop.Shared.Cache/"]
COPY ["EShop/Shared/src/EShop.Shared.Scoping/EShop.Shared.Scoping.csproj", "EShop/Shared/src/EShop.Shared.Scoping/"]
COPY ["EShop/Shared/src/EShop.Shared.Contracts/EShop.Shared.Contracts.csproj", "EShop/Shared/src/EShop.Shared.Contracts/"]
COPY ["EShop/Shared/src/EShop.Shared.DomainTools/EShop.Shared.DomainTools.csproj", "EShop/Shared/src/EShop.Shared.DomainTools/"]
COPY ["EShop/Shared/src/EShop.Shared.JsonApi/EShop.Shared.JsonApi.csproj", "EShop/Shared/src/EShop.Shared.JsonApi/"]
COPY ["EShop/Shared/src/EShop.Shared.DbResourceAccessControl/EShop.Shared.DbResourceAccessControl.csproj", "EShop/Shared/src/EShop.Shared.DbResourceAccessControl/"]
COPY ["EShop/Shared/src/EShop.Shared.Diagnostics/EShop.Shared.Diagnostics.csproj", "EShop/Shared/src/EShop.Shared.Diagnostics/"]

RUN dotnet restore "./EShop/ReverseProxy/src/ApiGateway/ApiGateway.csproj"

COPY . .

WORKDIR "/src/EShop/ReverseProxy/src/ApiGateway"
RUN dotnet build "./ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]