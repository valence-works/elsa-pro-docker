FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy central package management and solution files
COPY NuGet.config ./
COPY Directory.Packages.props ./
COPY ElsaProServer.sln ./

# Copy all project files for restore
COPY src/ElsaProStudio/ElsaProStudio.csproj src/ElsaProStudio/
COPY src/ElsaProStudio.Client/ElsaProStudio.Client.csproj src/ElsaProStudio.Client/
COPY src/ElsaProStudio.Server/ElsaProStudio.Server.csproj src/ElsaProStudio.Server/
COPY src/ElsaProStudio.Shared/ElsaProStudio.Shared.csproj src/ElsaProStudio.Shared/
COPY src/ElsaProServer.ServiceDefaults/ElsaProServer.ServiceDefaults.csproj src/ElsaProServer.ServiceDefaults/
COPY src/ElsaProServer/ElsaProServer.csproj src/ElsaProServer/
COPY src/ElsaProServer.Identity/ElsaProServer.Identity.csproj src/ElsaProServer.Identity/
COPY src/SamplePackage/SamplePackage.csproj src/SamplePackage/
COPY src/ElsaProServer.AppHost/ElsaProServer.AppHost.csproj src/ElsaProServer.AppHost/

# Restore dependencies
RUN dotnet restore src/ElsaProStudio/ElsaProStudio.csproj

# Copy source code
COPY src/ElsaProStudio/ src/ElsaProStudio/
COPY src/ElsaProStudio.Client/ src/ElsaProStudio.Client/
COPY src/ElsaProStudio.Server/ src/ElsaProStudio.Server/
COPY src/ElsaProStudio.Shared/ src/ElsaProStudio.Shared/
COPY src/ElsaProServer.ServiceDefaults/ src/ElsaProServer.ServiceDefaults/

# Publish
RUN dotnet publish src/ElsaProStudio/ElsaProStudio.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

RUN mkdir -p /config

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ElsaProStudio.dll"]
