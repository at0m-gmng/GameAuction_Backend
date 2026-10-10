using GameBackend.Services.Lobby.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameBackend.Services.Lobby.Infrastructure.Background;

/// <summary>
/// Фоновое завершение просроченных аукционов: расчёт проходит, даже если страницу лобби никто не открыл.
/// Дополняет ленивое завершение при чтении; каждое лобби обрабатывается в своей области DI и перепроверяется на свежих данных.
/// Пока сервис Render спит, проверка не идёт — тогда аукцион завершит первый запрос после пробуждения.
/// </summary>
public sealed class AuctionExpiryProcessor : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuctionExpiryProcessor> _logger;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="scopeFactory">Фабрика областей DI (контекст БД — scoped).</param>
    /// <param name="logger">Логгер.</param>
    public AuctionExpiryProcessor(IServiceScopeFactory scopeFactory, ILogger<AuctionExpiryProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Цикл проверки до остановки хоста.
    /// </summary>
    /// <param name="stoppingToken">Токен остановки.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(CheckInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CompleteExpiredAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Сбой фоновой проверки просроченных аукционов");
            }
        }
    }

    private async Task CompleteExpiredAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Guid> expired;
        using (IServiceScope scope = _scopeFactory.CreateScope())
        {
            expired = await scope.ServiceProvider.GetRequiredService<AuctionCompletionService>().GetExpiredLobbyIdsAsync(cancellationToken);
        }

        foreach (Guid lobbyId in expired)
        {
            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                await scope.ServiceProvider.GetRequiredService<AuctionCompletionService>().CompleteIfExpiredAsync(lobbyId, cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Не удалось завершить просроченный аукцион {LobbyId}; повтор на следующей проверке", lobbyId);
            }
        }
    }
}
