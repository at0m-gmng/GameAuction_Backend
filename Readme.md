# GameBackend

## Постановка задачи

Бэкенд для GameAuction — платформы живых аукционов за игровые предметы: игроки регистрируются, получают приветственный предмет и стартовый баланс, просматривают витрину, заходят в лобби и делают ставки в реальном времени за право забрать предмет в свой инвентарь.

## Связанные репозитории

- [GameAuction_Front](https://github.com/at0m-gmng/GameAuction_Front) (локально — `Nexus Exchange`) — фронтенд, единственный клиент этого бэкенда. Экраны и API-контракты должны рассматриваться вместе с этим репозиторием.

## Сервисы

| Сервис | Назначение | Статус |
|---|---|---|
| **Identity.API** | Регистрация/вход, выдача JWT, баланс игрока (`GoldCredits`), оркестрация приветственного подарка | Задеплоен |
| **Catalog.API** | Публичная витрина + приватный инвентарь игрока: покупка, выставление на продажу, снятие с продажи, выдача предмета победителю | Задеплоен |
| **Generation.API** | Процедурная генерация заготовок предметов (архетип × редкость) — вызывается только другими сервисами | Задеплоен |
| **Lobby.API** | Лобби, ставки, жизненный цикл аукциона, SignalR-хаб реального времени, расчёт с победителем и продавцом | Задеплоен |
| **SharedKernel** | Общие доменные типы (`ItemCategory`, `ItemRarity`) и абстракции Application (`IUnitOfWork`, `IProcessedOperationStore`) | Библиотека без внешних зависимостей |
| **ServiceDefaults** | Общая инфраструктура хоста: JWT, CORS, OpenAPI + Scalar, Serilog, correlation-id, проверка `X-Internal-Key`, EF-база с журналом идемпотентности | Библиотека, на которую ссылаются Infrastructure/API всех сервисов |

Витрина Catalog.API наполняется через Generation.API: инициализатор при старте держит в витрине заданное число публичных лотов (`Marketplace:PublicCatalogSeedCount`) — недостающие догенерирует, распроданные и лишние снимает с публикации. Фронтенд работает с реальными данными — каталог, инвентарь, лобби и ставки, моков нет.

Межсервисные вызовы (Identity → Catalog/Generation, Lobby → Identity/Catalog) идут напрямую по HTTP с общим секретом в заголовке `X-Internal-Key` (в проде — HTTPS через публичные URL Render; `http://localhost` в appsettings — только локально); очереди сообщений в проекте нет. Клиенты обёрнуты в resilience-пайплайн (retry + circuit breaker + timeout), мутирующие операции идемпотентны (ключ `Idempotency-Key` + журнал), а сквозной `X-Correlation-ID` связывает логи одного действия через все сервисы. События реального времени рассылаются через SignalR-хаб Lobby.API (`/hubs/lobby`), изменение баланса пушится адресно затронутому игроку.

У каждого сервиса есть health-check `/health` (проверка подключения к своей БД); у Identity/Catalog/Lobby — интерактивная API-документация `/scalar/v1` поверх OpenAPI (со схемой Bearer: токен из `/api/auth/login` вставляется в Scalar для вызова защищённых методов).

## Архитектура

Каждый сервис разбит по чистой архитектуре на проекты `Domain` → `Application` → `Infrastructure` → `API` (см. [ADR 0008](docs/adr/0008-clean-architecture-layers.md)).

C4-диаграммы (context, container) и обоснование ключевых решений — в [`docs/architecture.md`](docs/architecture.md). Осознанные архитектурные выборы (EnsureCreated вместо миграций, `X-Internal-Key`, отсутствие outbox, пуш баланса по SignalR) и **границы масштаба** — что сознательно вне объёма при текущем масштабе и как это развивать (RS256, трейсинг, кеш, Vault и т.п.) — зафиксированы как ADR в [`docs/adr/`](docs/adr/).

## Технологический стек

- .NET 10, ASP.NET Core Web API
- PostgreSQL (Neon) через EF Core + Npgsql (индексы на фильтруемых полях)
- JWT Bearer аутентификация (HS256)
- SignalR — события аукциона и баланса в реальном времени
- FluentValidation — валидация входных запросов на границе API
- `Microsoft.Extensions.Http.Resilience` (Polly) — retry + circuit breaker + timeout на межсервисных клиентах
- Идемпотентность межсервисных операций — заголовок `Idempotency-Key` + журнал `ProcessedOperations`
- Correlation ID — сквозной `X-Correlation-ID` через сервисы, обогащает логи
- Rate limiting (встроенный в .NET) на эндпоинтах входа/регистрации
- Serilog — структурное логирование (JSON) + request logging
- Scalar — UI API-документации поверх встроенного OpenAPI
- Health checks (EF Core)
- Docker
- xUnit — модульные тесты домена и интеграционные (`WebApplicationFactory`, SQLite in-memory)

## Деплой

Все четыре Web Service развёрнуты на [Render](https://render.com), у каждого свой Dockerfile: `src/Services/<Service>/Dockerfile` (стандартная многоэтапная сборка: restore → publish → `ENTRYPOINT ["dotnet", "<Service>.API.dll"]`). Контекст сборки — корень репозитория. Порт берётся из переменной `PORT`, которую задаёт Render. БД — PostgreSQL на [Neon](https://neon.tech), у каждого сервиса своя строка подключения.

Настройки сервиса в Render: **Dockerfile Path** — `./src/Services/<Service>/Dockerfile`, **Docker Build Context Directory** — `.`.

Локальная сборка образа:

```bash
docker build -f src/Services/Lobby/Dockerfile -t gameauction-lobby .
```

Все четыре сервиса деплоятся автоматически при пуше в `main` (git-интеграцией Render). GitHub Actions используется только для CI (сборка, тесты и сборка Docker-образов) — см. [`.github/workflows/build.yml`](.github/workflows/build.yml).

## Локальный запуск

Секреты не хранятся в репозитории. После клонирования для Identity.API:

```powershell
# Сгенерировать 32 случайных байта в base64 (годится как секрет)
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))

dotnet user-secrets init --project src/Services/Identity/Identity.API/Identity.API.csproj
dotnet user-secrets set "Jwt:SecretKey" "<значение из менеджера паролей>"
```

Требуемые секреты (по одному на сервис, где используются): `Jwt:SecretKey`, `InternalApi:Key`.
