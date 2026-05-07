FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY NuGet.config ./
COPY Directory.Packages.props ./
COPY ElsaProServer.sln ./

COPY src/ElsaProCombined/ElsaProCombined.csproj src/ElsaProCombined/
COPY src/ElsaProCombined.Client/ElsaProCombined.Client.csproj src/ElsaProCombined.Client/
COPY src/ElsaProServer.Identity/ElsaProServer.Identity.csproj src/ElsaProServer.Identity/
COPY src/ElsaProServer.ServiceDefaults/ElsaProServer.ServiceDefaults.csproj src/ElsaProServer.ServiceDefaults/

RUN dotnet restore src/ElsaProCombined/ElsaProCombined.csproj

COPY src/ElsaProCombined/ src/ElsaProCombined/
COPY src/ElsaProCombined.Client/ src/ElsaProCombined.Client/
COPY src/ElsaProServer.Identity/ src/ElsaProServer.Identity/
COPY src/ElsaProServer.ServiceDefaults/ src/ElsaProServer.ServiceDefaults/

RUN dotnet publish src/ElsaProCombined/ElsaProCombined.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

RUN apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

RUN mkdir -p /config /app/packages

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ElsaProCombined.dll"]
