# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["AjkTransportSystem.API/AjkTransportSystem.API.csproj", "AjkTransportSystem.API/"]
RUN dotnet restore "AjkTransportSystem.API/AjkTransportSystem.API.csproj"

# Copy full source and publish release build
COPY . .
WORKDIR "/src/AjkTransportSystem.API"
RUN dotnet publish "AjkTransportSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose default HTTP port
EXPOSE 80

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=80

ENTRYPOINT ["dotnet", "AjkTransportSystem.API.dll"]

