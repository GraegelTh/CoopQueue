# 1. Build Stage: Use the SDK image for compiling
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files (to allow for 'dotnet restore')
COPY ["CoopQueue.Api/CoopQueue.Api.csproj", "CoopQueue.Api/"]
COPY ["CoopQueue.Shared/CoopQueue.Shared.csproj", "CoopQueue.Shared/"]
COPY ["CoopQueue.Client/CoopQueue.Client.csproj", "CoopQueue.Client/"]

# Restore dependencies
RUN dotnet restore "CoopQueue.Api/CoopQueue.Api.csproj"

# Copy the remaining source code
COPY . .

# Build the project
WORKDIR "/src/CoopQueue.Api"
RUN dotnet build "CoopQueue.Api.csproj" -c Release -o /app/build

# Publish the project (Pack everything into one folder)
FROM build AS publish
RUN dotnet publish "CoopQueue.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Runtime Stage: The actual image for execution (smaller & faster)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoopQueue.Api.dll"]