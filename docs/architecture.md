# Архитектура GameAuction

Диаграммы в нотации [C4](https://c4model.com/). Рендерятся на GitHub через Mermaid.

## Уровень 1 — System Context

```mermaid
C4Context
    title System Context — GameAuction
    Person(player, "Игрок", "Регистрируется, торгуется на аукционах")
    System(front, "Nexus Exchange", "React SPA на GitHub Pages")
    System(backend, "GameBackend", "Микросервисы .NET 10")
    SystemDb(neon, "Neon PostgreSQL", "Отдельная БД на каждый сервис")

    Rel(player, front, "Пользуется", "HTTPS")
    Rel(front, backend, "Вызывает API, слушает realtime", "HTTPS/JWT, WebSocket")
    Rel(backend, neon, "Читает/пишет", "TCP")
```

## Уровень 2 — Containers

```mermaid
C4Container
    title Container Diagram — GameBackend
    Person(player, "Игрок")
    Container(spa, "Nexus Exchange", "React 19, TypeScript", "SPA, GitHub Pages")

    Container_Boundary(be, "GameBackend") {
        Container(identity, "Identity.API", ".NET 10", "Регистрация/вход, JWT, баланс")
        Container(catalog, "Catalog.API", ".NET 10", "Витрина, инвентарь, покупка/продажа")
        Container(generation, "Generation.API", ".NET 10", "Генерация заготовок предметов")
        Container(lobby, "Lobby.API", ".NET 10", "Аукционы, ставки, SignalR-хаб")
    }
    ContainerDb(neon, "Neon PostgreSQL", "PostgreSQL", "identity_db, catalog_db, generation_db, lobby_db")

    Rel(player, spa, "Пользуется", "HTTPS")
    Rel(spa, identity, "Аутентификация, профиль", "HTTPS/JWT")
    Rel(spa, catalog, "Витрина, покупка, листинг", "HTTPS/JWT")
    Rel(spa, lobby, "Ставки, живой аукцион", "HTTPS/JWT, WebSocket")

    Rel(identity, generation, "Сгенерировать подарок", "HTTP + X-Internal-Key")
    Rel(identity, catalog, "Выдать подарок игроку", "HTTP + X-Internal-Key")
    Rel(catalog, generation, "Наполнить витрину", "HTTP + X-Internal-Key")
    Rel(catalog, lobby, "Создать лобби", "HTTP + X-Internal-Key")
    Rel(catalog, identity, "Списать за покупку", "HTTP + X-Internal-Key")
    Rel(lobby, catalog, "Передать предмет победителю", "HTTP + X-Internal-Key")
    Rel(lobby, identity, "Списать/начислить, баланс", "HTTP + X-Internal-Key")
    Rel(lobby, spa, "События аукциона и баланса", "SignalR/WebSocket")

    Rel(identity, neon, "EF Core")
    Rel(catalog, neon, "EF Core")
    Rel(generation, neon, "EF Core")
    Rel(lobby, neon, "EF Core")
```

## Ключевые решения

Причины неочевидных архитектурных выборов зафиксированы в ADR — см. [`docs/adr/`](adr/):

- [0001](adr/0001-ensurecreated-over-ef-migrations.md) — `EnsureCreated` + идемпотентные патчи вместо EF Migrations
- [0002](adr/0002-internal-key-service-auth.md) — `X-Internal-Key` для межсервисной аутентификации
- [0003](adr/0003-in-memory-events-no-outbox.md) — доменные события через SignalR, без очереди и outbox
- [0004](adr/0004-signalr-balance-push.md) — пуш баланса по SignalR вместо опроса
