FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY . .
RUN dotnet publish src/Services/Identity.API/Identity.API.csproj -c Release -o /out/identity && \
    dotnet publish src/Services/Catalog.API/Catalog.API.csproj -c Release -o /out/catalog

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
# Запуск из папки сервиса, чтобы content root совпал с его appsettings.json.
CMD cd "$(dirname "$SERVICE_PATH")" && ASPNETCORE_URLS=http://0.0.0.0:$PORT dotnet "./$(basename "$SERVICE_PATH")"