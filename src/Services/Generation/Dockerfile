FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["src/Shared/SharedKernel/SharedKernel.csproj", "src/Shared/SharedKernel/"]
COPY ["src/Shared/ServiceDefaults/ServiceDefaults.csproj", "src/Shared/ServiceDefaults/"]
COPY ["src/Services/Generation/Generation.Domain/Generation.Domain.csproj", "src/Services/Generation/Generation.Domain/"]
COPY ["src/Services/Generation/Generation.Application/Generation.Application.csproj", "src/Services/Generation/Generation.Application/"]
COPY ["src/Services/Generation/Generation.Infrastructure/Generation.Infrastructure.csproj", "src/Services/Generation/Generation.Infrastructure/"]
COPY ["src/Services/Generation/Generation.API/Generation.API.csproj", "src/Services/Generation/Generation.API/"]
RUN dotnet restore "src/Services/Generation/Generation.API/Generation.API.csproj"
COPY ["src/Shared/", "src/Shared/"]
COPY ["src/Services/Generation/", "src/Services/Generation/"]
RUN dotnet publish "src/Services/Generation/Generation.API/Generation.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Generation.API.dll"]
