using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameBackend.Services.Lobby.Infrastructure.Outbox;

/// <summary>
/// Фоновый обработчик outbox расчётов внутри Lobby.API: просыпается по сигналу нового расчёта или по интервалу.
/// Пока сервис Render спит, расчёты ждут в БД и уходят после пробуждения.
/// </summary>
public sealed class SettlementOutboxProcessor : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SettlementOutboxSignal _signal;
    private readonly ILogger<SettlementOutboxProcessor> _logger;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="scopeFactory">Фабрика областей DI (контекст БД — scoped).</param>
    /// <param name="signal">Сигнал о новых расчётах.</param>
    /// <param name="logger">Логгер.</param>
    public SettlementOutboxProcessor(IServiceScopeFactory scopeFactory, SettlementOutboxSignal signal, ILogger<SettlementOutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _signal = signal;
        _logger = logger;
    }

    /// <summary>
    /// Цикл обработки до остановки хоста.
    /// </summary>
    /// <param name="stoppingToken">Токен остановки.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            int processed;

            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                processed = await scope.ServiceProvider.GetRequiredService<SettlementOutboxDispatcher>().DispatchDueAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Сбой обработки outbox расчётов");
                processed = 0;
            }

            if (processed < SettlementOutboxDispatcher.BatchSize)
            {
                await _signal.WaitAsync(PollInterval, stoppingToken);
            }
        }
    }
}
