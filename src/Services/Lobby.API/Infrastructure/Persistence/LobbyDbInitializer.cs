using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameBackend.Services.Lobby.API.Infrastructure.Persistence;

/// <summary>
/// Инициализатор базы данных лобби: создаёт схему и донакатывает изменения на уже существующую таблицу.
/// </summary>
public static class LobbyDbInitializer
{
    /// <summary>
    /// Создаёт схему БД (для новой базы) и применяет идемпотентные патчи схемы (для уже существующей).
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    /// <param name="logger">Логгер для best-effort патчей, которые не должны ронять запуск сервиса.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public static async Task InitializeAsync(LobbyDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        // NOTE: EnsureCreated не донакатывает индексы/колонки в уже существующую таблицу — патч идемпотентен.
        await context.Database.EnsureCreatedAsync(cancellationToken);

        if (!context.Database.IsNpgsql())
            return;

        try
        {
            await context.Database.ExecuteSqlRawAsync(
                """
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_Lobbies_ItemId_Status"
                ON "Lobbies" ("ItemId", "Status")
                WHERE "Status" IN (100, 200);
                """,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // NOTE: best-effort — старый дубль в данных не должен ронять запуск; race тогда остаётся открытой.
            logger.LogWarning(ex, "Не удалось создать индекс IX_Lobbies_ItemId_Status — защита от гонки не активна");
        }
    }
}
