#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY src/*.csproj .
RUN dotnet restore
COPY src/. .
RUN dotnet build -c Release -o /app/build --framework net6.0

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish --framework net6.0

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AdsRouterConsoleApp.dll"]