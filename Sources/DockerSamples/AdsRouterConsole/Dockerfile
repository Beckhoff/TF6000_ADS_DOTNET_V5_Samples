# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["./NuGet.Config", "AdsRouterConsole/"]
COPY ["AdsRouterConsole/AdsRouterConsole.csproj", "AdsRouterConsole/"]

ARG BUILD_CONFIGURATION=Release

# Access to DevOps Server Feed TcBase (Beckhoff internal)
ARG NuGetPackageSourceCredentials_TcBase
ENV NuGetPackageSourceCredentials_TcBase=$NuGetPackageSourceCredentials_TcBase
ARG PAT
ENV PAT=$PAT
RUN echo "NuGetPackageSourceCredentials_TcBase: $NuGetPackageSourceCredentials_TcBase"
RUN echo "PAT: $PAT"

COPY . .
WORKDIR "/src/AdsRouterConsole"

# Clear all nuget caches
RUN dotnet nuget locals all --clear
RUN dotnet build "./AdsRouterConsole.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
RUN dotnet publish "./AdsRouterConsole.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 48898/tcp
EXPOSE 48899/udp

ENTRYPOINT ["dotnet", "AdsRouterConsole.dll"]