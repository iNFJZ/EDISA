# Multi-stage build for Microservice System
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and all project files first
COPY MicroserviceSystem.sln ./
COPY AuthService/AuthService.csproj AuthService/
COPY FileService/FileService.csproj FileService/
COPY GatewayApi/GatewayApi.csproj GatewayApi/
COPY EmailService/EmailService.csproj EmailService/
COPY UserService/UserService.csproj UserService/
COPY NotificationService/NotificationService.csproj NotificationService/
COPY WorkerService/WorkerService.csproj WorkerService/
COPY Shared/Shared.csproj Shared/
COPY nuget.config ./

# Restore dependencies for all projects
RUN dotnet restore "Shared/Shared.csproj" && \
    dotnet restore "AuthService/AuthService.csproj" && \
    dotnet restore "FileService/FileService.csproj" && \
    dotnet restore "GatewayApi/GatewayApi.csproj" && \
    dotnet restore "EmailService/EmailService.csproj" && \
    dotnet restore "UserService/UserService.csproj" && \
    dotnet restore "NotificationService/NotificationService.csproj" && \
    dotnet restore "WorkerService/WorkerService.csproj"

# Copy the rest of the source code
COPY . .

# Build all projects
RUN dotnet build "AuthService/AuthService.csproj" -c Release --no-restore && \
    dotnet build "FileService/FileService.csproj" -c Release --no-restore && \
    dotnet build "GatewayApi/GatewayApi.csproj" -c Release --no-restore && \
    dotnet build "EmailService/EmailService.csproj" -c Release --no-restore && \
    dotnet build "UserService/UserService.csproj" -c Release --no-restore && \
    dotnet build "NotificationService/NotificationService.csproj" -c Release --no-restore && \
    dotnet build "WorkerService/WorkerService.csproj" -c Release --no-restore

# Publish all projects
RUN dotnet publish "AuthService/AuthService.csproj" -c Release -o /app/publish/AuthService && \
    dotnet publish "FileService/FileService.csproj" -c Release -o /app/publish/FileService && \
    dotnet publish "GatewayApi/GatewayApi.csproj" -c Release -o /app/publish/GatewayApi && \
    dotnet publish "EmailService/EmailService.csproj" -c Release -o /app/publish/EmailService && \
    dotnet publish "UserService/UserService.csproj" -c Release -o /app/publish/UserService && \
    dotnet publish "NotificationService/NotificationService.csproj" -c Release -o /app/publish/NotificationService && \
    dotnet publish "WorkerService/WorkerService.csproj" -c Release -o /app/publish/WorkerService

# Final stage: Copy published applications and configuration files
FROM base AS final
WORKDIR /app

# Create logs directories for all services
RUN mkdir -p /app/logs /app/AuthService/logs /app/FileService/logs /app/UserService/logs /app/EmailService/logs /app/NotificationService/logs

# Copy published applications
COPY --from=build /app/publish/AuthService /app/AuthService
COPY --from=build /app/publish/FileService /app/FileService
COPY --from=build /app/publish/GatewayApi /app/GatewayApi
COPY --from=build /app/publish/EmailService /app/EmailService
COPY --from=build /app/publish/UserService /app/UserService
COPY --from=build /app/publish/NotificationService /app/NotificationService
COPY --from=build /app/publish/WorkerService /app/WorkerService

# Copy configuration files for AuthService
COPY AuthService/appsettings.json /app/AuthService/appsettings.json
COPY AuthService/appsettings.Development.json /app/AuthService/appsettings.Development.json

# Copy configuration files for FileService
COPY FileService/appsettings.json /app/FileService/appsettings.json
COPY FileService/appsettings.Development.json /app/FileService/appsettings.Development.json

# Copy configuration files for EmailService
COPY EmailService/appsettings.json /app/EmailService/appsettings.json
COPY EmailService/appsettings.Development.json /app/EmailService/appsettings.Development.json

# Copy configuration files for UserService
COPY UserService/appsettings.json /app/UserService/appsettings.json
COPY UserService/appsettings.Development.json /app/UserService/appsettings.Development.json

# Copy configuration files for NotificationService
COPY NotificationService/appsettings.json /app/NotificationService/appsettings.json
COPY NotificationService/appsettings.Development.json /app/NotificationService/appsettings.Development.json

# Copy ocelot.json for GatewayApi
COPY GatewayApi/ocelot.json /app/GatewayApi/ocelot.json

# Copy language files for all services
COPY Shared/LanguageFiles /app/Shared/LanguageFiles
COPY Shared/LanguageFiles /app/NotificationService/LanguageFiles

# Copy configuration files for WorkerService
COPY WorkerService/appsettings.json /app/WorkerService/appsettings.json
COPY WorkerService/appsettings.Development.json /app/WorkerService/appsettings.Development.json