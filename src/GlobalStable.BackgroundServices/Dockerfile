#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY GlobalStable.BackgroundServices/GlobalStable.BackgroundServices.csproj GlobalStable.BackgroundServices/
COPY GlobalStable.Application/GlobalStable.Application.csproj GlobalStable.Application/
COPY GlobalStable.Domain/GlobalStable.Domain.csproj GlobalStable.Domain/
COPY GlobalStable.Infrastructure/GlobalStable.Infrastructure.csproj GlobalStable.Infrastructure/

RUN dotnet restore "./GlobalStable.BackgroundServices/GlobalStable.BackgroundServices.csproj"

COPY . .

WORKDIR "/src/GlobalStable.BackgroundServices"
RUN dotnet build "./GlobalStable.BackgroundServices.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "./GlobalStable.BackgroundServices.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final

USER root
RUN apt-get update && apt-get install -y wget ca-certificates gnupg \
    && echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
    && wget https://download.newrelic.com/548C16BF.gpg \
    && apt-key add 548C16BF.gpg \
    && apt-get update \
    && apt-get install -y newrelic-dotnet-agent

ENV CORECLR_ENABLE_PROFILING=1 \
    CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
    CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
    CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so \
    NEW_RELIC_LICENSE_KEY=40a047fa29fe968eac553d0aa97abda3FFFFNRAL \
    NEW_RELIC_APP_NAME=bgx-order-service-worker

WORKDIR /app
COPY --from=publish /app/publish .

USER app

ENTRYPOINT ["dotnet", "GlobalStable.BackgroundServices.dll"]