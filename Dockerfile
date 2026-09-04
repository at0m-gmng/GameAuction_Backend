FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY . .
RUN dotnet publish src/Services/Identity.API/Identity.API.csproj -c Release -o /out/identity && \
    dotnet publish src/Services/Catalog.API/Catalog.API.csproj -c Release -o /out/catalog

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
# Один образ на все сервисы, SERVICE_PATH выбирает нужный. Запускаем из его папки,
# чтобы content root совпал с местом appsettings.json — иначе из /app он не грузится
# и Jwt:Issuer/Audience молча пусты.
CMD cd "$(dirname "$SERVICE_PATH")" && ASPNETCORE_URLS=http://0.0.0.0:$PORT dotnet "./$(basename "$SERVICE_PATH")"