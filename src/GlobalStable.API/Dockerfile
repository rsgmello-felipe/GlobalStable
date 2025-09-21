#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY GlobalStable.API/GlobalStable.API.csproj GlobalStable.API/
COPY GlobalStable.Application/GlobalStable.Application.csproj GlobalStable.Application/
COPY GlobalStable.Domain/GlobalStable.Domain.csproj GlobalStable.Domain/
COPY GlobalStable.Infrastructure/GlobalStable.Infrastructure.csproj GlobalStable.Infrastructure/

RUN dotnet restore GlobalStable.API/GlobalStable.API.csproj

COPY . .

WORKDIR "/src/GlobalStable.API"
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER root

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GlobalStable.API.dll"]