using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GameBackend.Services.Lobby.Infrastructure.Persistence;

/// <summary>
/// Инициализатор базы данных лобби: создаёт схему и донакатывает изменения на уже существующую таблицу.
/// </summary>
public static class LobbyDbInitializer
{
    private const string CreateOpenLobbyUniqueIndexSql =
        """
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_Lobbies_ItemId_Status"
        ON "Lobbies" ("ItemId", "Status")
        WHERE "Status" IN (100, 200);
        """;

    private const string CreateSettlementMessagesSql =
        """
        CREATE TABLE IF NOT EXISTS "SettlementMessages" (
            "Id" uuid NOT NULL,
            "LobbyId" uuid NOT NULL,
            "ItemId" uuid NOT NULL,
            "WinnerId" uuid NOT NULL,
            "Amount" numeric(18,2) NOT NULL,
            "CreatedAt" timestamp with time zone NOT NULL,
            "NextAttemptAt" timestamp with time zone NOT NULL,
            "ProcessedAt" timestamp with time zone,
            "Attempts" integer NOT NULL,
            "LastError" character varying(2000),
            CONSTRAINT "PK_SettlementMessages" PRIMARY KEY ("Id")
        );
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_SettlementMessages_LobbyId" ON "SettlementMessages" ("LobbyId");
        CREATE INDEX IF NOT EXISTS "IX_SettlementMessages_ProcessedAt_NextAttemptAt" ON "SettlementMessages" ("ProcessedAt", "NextAttemptAt");
        """;

    /// <summary>
    /// Создаёт область DI, резолвит контекст и выполняет <see cref="InitializeAsync"/>.
    /// </summary>
    /// <param name="services">Корневой провайдер сервисов.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public static async Task InitializeLobbyDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = services.CreateScope();
        await InitializeAsync(
            scope.ServiceProvider.GetRequiredService<LobbyDbContext>(),
            scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(LobbyDbInitializer)),
            cancellationToken);
    }

    /// <summary>
    /// Создаёт схему БД (для новой базы) и применяет идемпотентные патчи схемы (для уже существующей).
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    /// <param name="logger">Логгер для best-effort патчей, которые не должны ронять запуск сервиса.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public static async Task InitializeAsync(LobbyDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        await context.Database.EnsureCreatedAsync(cancellationToken);

        if (context.Database.IsNpgsql())
        {
            await context.Database.ExecuteSqlRawAsync(CreateSettlementMessagesSql, cancellationToken);

            try
            {
                await context.Database.ExecuteSqlRawAsync(CreateOpenLobbyUniqueIndexSql, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Не удалось создать индекс IX_Lobbies_ItemId_Status — защита от гонки не активна");
            }
        }
    }
}
