# ADR 0008 — Чистая архитектура: слои сервиса — отдельные проекты

**Статус:** принято

## Контекст

Слои Domain/Application/Infrastructure были папками внутри одного API-проекта. Направление зависимостей ничем не проверялось и уже нарушалось: обработчики Application использовали `DbContext`, EF Core, HTTP-клиенты и `IHubContext` напрямую. Регистрация сервисов, JWT, CORS и логирование копировались в `Program.cs` каждого сервиса.

## Решение

Каждый сервис разбит на четыре проекта — компилятор не даёт нарушить направление зависимостей:

```
src/Services/<Service>/
  <Service>.Domain          агрегаты, value objects, доменные события      → SharedKernel
  <Service>.Application     команды/запросы, порты (интерфейсы)           → Domain
  <Service>.Infrastructure  EF Core, репозитории, HTTP-клиенты, настройки → Application, ServiceDefaults
  <Service>.API             контроллеры, валидаторы, SignalR, Program     → Infrastructure
src/Shared/
  SharedKernel              общие доменные типы и абстракции Application (без внешних зависимостей)
  ServiceDefaults           общая инфраструктура хоста: JWT, CORS, OpenAPI, Serilog, correlation-id, EF-база
```

- Application зависит только от портов: `IUnitOfWork` (транзакция), `IProcessedOperationStore` (журнал идемпотентности), клиенты других сервисов, `IPasswordHasher`/`ITokenGenerator`, `IPlayerNotifier`. Реализации — в Infrastructure или API (SignalR).
- Нарушение уникальности при сохранении переводится базовым `ServiceDbContext` в `DuplicateEntryException`, поэтому Application не знает про `DbUpdateException`.
- Каждый слой регистрирует себя сам (`Add<Service>Application`, `Add<Service>Infrastructure`); `Program.cs` только собирает слои.
- Настройки — `sealed record` с `init`, проверяются DataAnnotations при старте (`ValidateOnStart`) вместо ручных `if/throw`.

## Последствия

**Плюсы:** зависимости проверяет компилятор; Application тестируется без БД и HTTP; общая инфраструктура в одном месте.

**Минусы:** больше проектов (4 на сервис). Отдельные solution на сервис не вводим: `SharedKernel`/`ServiceDefaults` пришлось бы публиковать NuGet-пакетами — для монорепозитория это лишняя цена.
