using GameBackend.Services.Lobby.Domain;
using GameBackend.ServiceDefaults.Persistence;
using GameBackend.Services.Lobby.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Services.Lobby.Infrastructure.Persistence;

/// <summary>
/// Контекст базы данных лобби.
/// </summary>
public class LobbyDbContext : ServiceDbContext
{
    /// <summary>
    /// Инициализирует контекст с указанными опциями.
    /// </summary>
    /// <param name="options">Опции подключения к БД.</param>
    public LobbyDbContext(DbContextOptions<LobbyDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Таблица лобби.
    /// </summary>
    public DbSet<LobbyAggregate> Lobbies => Set<LobbyAggregate>();

    /// <summary>
    /// Outbox расчётов по завершённым аукционам.
    /// </summary>
    public DbSet<SettlementMessage> SettlementMessages => Set<SettlementMessage>();

    /// <summary>
    /// Применяет конфигурации маппинга из сборки.
    /// </summary>
    /// <param name="modelBuilder">Построитель модели.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LobbyDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}