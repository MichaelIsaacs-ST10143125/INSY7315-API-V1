# Use official ASP.NET Core runtime as base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["NewDawnPropertiesApi-V1.csproj", "./"]
RUN dotnet restore "NewDawnPropertiesApi-V1.csproj"

# Copy all source code
COPY . .

# Build
RUN dotnet build "NewDawnPropertiesApi-V1.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "NewDawnPropertiesApi-V1.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Render environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Start the app
ENTRYPOINT ["dotnet", "NewDawnPropertiesApi-V1.dll"]
