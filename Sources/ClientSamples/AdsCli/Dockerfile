#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY src/*.csproj .
RUN dotnet restore
COPY src/. .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app

ENV AmsConfiguration:LoopbackAddress=127.0.0.1 \
    AmsConfiguration:LoopbackPort=48898

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AdsCli.dll"]