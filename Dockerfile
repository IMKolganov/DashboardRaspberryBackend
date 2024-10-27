# Use the SDK image for building and running the app
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
USER app
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install curl for debugging
RUN apt-get update && apt-get install -y curl

# Use the SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DashboardRaspberryBackend.csproj", "."]
RUN dotnet restore "./DashboardRaspberryBackend.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "DashboardRaspberryBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DashboardRaspberryBackend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy appsettings files
COPY appsettings.json .
COPY appsettings.Development.json .
COPY appsettings.Docker.json .

ENTRYPOINT ["dotnet", "DashboardRaspberryBackend.dll"]
