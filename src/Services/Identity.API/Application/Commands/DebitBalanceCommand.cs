using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Identity.API.Application.Commands;

/// <summary>
/// Команда списания средств с баланса игрока (например, победа в аукционе).
/// </summary>
/// <param name="PlayerId">Идентификатор игрока.</param>
/// <param name="Amount">Сумма списания.</param>
public sealed record DebitBalanceCommand(Guid PlayerId, decimal Amount) : ICommand;
