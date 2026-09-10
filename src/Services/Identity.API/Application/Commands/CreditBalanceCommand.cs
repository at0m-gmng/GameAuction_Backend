using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Identity.API.Application.Commands;

/// <summary>
/// Команда пополнения баланса игрока (например, получение средств от продажи на аукционе).
/// </summary>
/// <param name="PlayerId">Идентификатор игрока.</param>
/// <param name="Amount">Сумма пополнения.</param>
public sealed record CreditBalanceCommand(Guid PlayerId, decimal Amount) : ICommand;
