FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy central package management and solution files
COPY NuGet.config ./
COPY Directory.Packages.props ./
COPY ElsaProServer.sln ./

# Copy all project files for restore
COPY src/ElsaProServer/ElsaProServer.csproj src/ElsaProServer/
COPY src/ElsaProServer.Identity/ElsaProServer.Identity.csproj src/ElsaProServer.Identity/
COPY src/ElsaProServer.ServiceDefaults/ElsaProServer.ServiceDefaults.csproj src/ElsaProServer.ServiceDefaults/
COPY src/SamplePackage/SamplePackage.csproj src/SamplePackage/
COPY src/ElsaProStudio.BlazorServer/ElsaProStudio.BlazorServer.csproj src/ElsaProStudio.BlazorServer/
COPY src/ElsaProServer.AppHost/ElsaProServer.AppHost.csproj src/ElsaProServer.AppHost/

# Restore dependencies
RUN dotnet restore src/ElsaProServer/ElsaProServer.csproj

# Copy source code
COPY src/ElsaProServer/ src/ElsaProServer/
COPY src/ElsaProServer.Identity/ src/ElsaProServer.Identity/
COPY src/ElsaProServer.ServiceDefaults/ src/ElsaProServer.ServiceDefaults/

# Publish
RUN dotnet publish src/ElsaProServer/ElsaProServer.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

RUN apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Create directories for config and packages
RUN mkdir -p /config /app/packages

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ElsaProServer.dll"]
