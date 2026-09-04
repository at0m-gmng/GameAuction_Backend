FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY . .
RUN dotnet publish src/Services/Identity.API/Identity.API.csproj -c Release -o /out/identity && \
    dotnet publish src/Services/Catalog.API/Catalog.API.csproj -c Release -o /out/catalog

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
# Каждый сервis лежит в своей подпапке (identity/, catalog/), а один и тот же
# образ выбирает нужный через SERVICE_PATH. Запускаем из папки самого сервиса,
# чтобы content root ASP.NET Core (по умолчанию — рабочая директория) совпадал с
# местом, где лежит его appsettings.json. Иначе из общего /app файл не находится
# и настройки вроде Jwt:Issuer / Jwt:Audience молча биндятся в пустоту.
CMD cd "$(dirname "$SERVICE_PATH")" && ASPNETCORE_URLS=http://0.0.0.0:$PORT dotnet "./$(basename "$SERVICE_PATH")"