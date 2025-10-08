# Use the official ASP.NET Core runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore dependencies
COPY ["NewDawnPropertiesApi-V1.csproj", "./"]
RUN dotnet restore "NewDawnPropertiesApi-V1.csproj"

# Copy all source code
COPY . .

# Build the application
RUN dotnet build "NewDawnPropertiesApi-V1.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "NewDawnPropertiesApi-V1.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Environment variables for Render
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Start the app
ENTRYPOINT ["dotnet", "NewDawnPropertiesApi-V1.dll"]
