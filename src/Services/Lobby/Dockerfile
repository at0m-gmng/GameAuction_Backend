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
COPY ["src/Services/Lobby/Lobby.Domain/Lobby.Domain.csproj", "src/Services/Lobby/Lobby.Domain/"]
COPY ["src/Services/Lobby/Lobby.Application/Lobby.Application.csproj", "src/Services/Lobby/Lobby.Application/"]
COPY ["src/Services/Lobby/Lobby.Infrastructure/Lobby.Infrastructure.csproj", "src/Services/Lobby/Lobby.Infrastructure/"]
COPY ["src/Services/Lobby/Lobby.API/Lobby.API.csproj", "src/Services/Lobby/Lobby.API/"]
RUN dotnet restore "src/Services/Lobby/Lobby.API/Lobby.API.csproj"
COPY ["src/Shared/", "src/Shared/"]
COPY ["src/Services/Lobby/", "src/Services/Lobby/"]
RUN dotnet publish "src/Services/Lobby/Lobby.API/Lobby.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Lobby.API.dll"]
