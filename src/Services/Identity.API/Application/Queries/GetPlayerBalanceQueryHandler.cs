using GameBackend.Services.Identity.API.Application.Interfaces;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Identity.API.Application.Queries;

/// <summary>
/// Обработчик запроса баланса игрока.
/// </summary>
public sealed class GetPlayerBalanceQueryHandler : IQueryHandler<GetPlayerBalanceQuery, decimal>
{
    private readonly IPlayerRepository _repository;

    /// <summary>
    /// Инициализирует обработчик репозиторием.
    /// </summary>
    /// <param name="repository">Репозиторий игроков.</param>
    public GetPlayerBalanceQueryHandler(IPlayerRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Загружает игрока и возвращает его текущий баланс.
    /// </summary>
    /// <param name="query">Запрос.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<decimal> Handle(GetPlayerBalanceQuery query, CancellationToken cancellationToken)
    {
        var player = await _repository.GetAsync(query.PlayerId, cancellationToken)
                     ?? throw new InvalidOperationException($"Игрок {query.PlayerId} не найден");

        return player.Balance.Amount;
    }
}
