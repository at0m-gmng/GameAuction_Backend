using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Identity.API.Application.Commands;

/// <summary>
/// Команда списания средств с баланса игрока (например, победа в аукционе).
/// </summary>
/// <param name="IdempotencyKey">Ключ идемпотентности — повтор с тем же ключом не списывает дважды.</param>
/// <param name="PlayerId">Идентификатор игрока.</param>
/// <param name="Amount">Сумма списания.</param>
public sealed record DebitBalanceCommand(string IdempotencyKey, Guid PlayerId, decimal Amount) : ICommand;
