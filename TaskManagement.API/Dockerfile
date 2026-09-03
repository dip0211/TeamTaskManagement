# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files for layer caching
COPY ["src/TaskManagement.Core/TaskManagement.Core.csproj", "src/TaskManagement.Core/"]
COPY ["src/TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj", "src/TaskManagement.Infrastructure/"]
COPY ["src/TaskManagement.API/TaskManagement.API.csproj", "src/TaskManagement.API/"]

RUN dotnet restore "src/TaskManagement.API/TaskManagement.API.csproj"

# Copy everything else and publish
COPY . .
WORKDIR "/src/src/TaskManagement.API"
RUN dotnet publish "TaskManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TaskManagement.API.dll"]