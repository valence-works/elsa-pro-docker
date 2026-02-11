# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["ElsaProServer.sln", "./"]
COPY ["src/ElsaProServer/ElsaProServer.csproj", "src/ElsaProServer/"]

# Restore dependencies
RUN dotnet restore "ElsaProServer.sln"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/src/ElsaProServer"
RUN dotnet build "ElsaProServer.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ElsaProServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "ElsaProServer.dll"]
