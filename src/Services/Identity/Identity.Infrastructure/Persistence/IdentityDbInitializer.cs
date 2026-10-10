using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GameBackend.Services.Identity.Infrastructure.Persistence;

/// <summary>
/// Инициализатор базы данных идентификации: EnsureCreated + идемпотентные патчи схемы старой БД (ADR 0001).
/// </summary>
public static class IdentityDbInitializer
{
    private const string AddWelcomeGiftColumnSql =
        """ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "WelcomeGiftGranted" boolean NOT NULL DEFAULT false;""";

    private const string AddStartingBalanceColumnSql =
        """ALTER TABLE "Players" ADD COLUMN IF NOT EXISTS "StartingBalanceGranted" boolean NOT NULL DEFAULT false;""";

    private const string DropOrphanInventoryColumnSql =
        """ALTER TABLE "Players" DROP COLUMN IF EXISTS "Inventory";""";

    private const string CreateProcessedOperationsSql =
        """
        CREATE TABLE IF NOT EXISTS "ProcessedOperations" (
            "Key" text NOT NULL PRIMARY KEY,
            "ProcessedAt" timestamp with time zone NOT NULL
        );
        """;

    private const string AddProcessedOperationResultColumnSql =
        """ALTER TABLE "ProcessedOperations" ADD COLUMN IF NOT EXISTS "Result" text NULL;""";

    /// <summary>
    /// Создаёт схему и доливает колонки/таблицы, которых EnsureCreated не добавляет в существующую БД.
    /// StartingBalanceGranted по умолчанию false: старые игроки дополучат баланс при следующем входе.
    /// </summary>
    /// <param name="services">Корневой провайдер сервисов.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public static async Task InitializeIdentityDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = services.CreateScope();
        IdentityDbContext context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        await context.Database.EnsureCreatedAsync(cancellationToken);

        if (context.Database.IsNpgsql())
        {
            await context.Database.ExecuteSqlRawAsync(AddWelcomeGiftColumnSql, cancellationToken);
            await context.Database.ExecuteSqlRawAsync(AddStartingBalanceColumnSql, cancellationToken);
            await context.Database.ExecuteSqlRawAsync(DropOrphanInventoryColumnSql, cancellationToken);
            await context.Database.ExecuteSqlRawAsync(CreateProcessedOperationsSql, cancellationToken);
            await context.Database.ExecuteSqlRawAsync(AddProcessedOperationResultColumnSql, cancellationToken);
        }
    }
}
