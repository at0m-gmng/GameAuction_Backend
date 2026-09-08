using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Identity.API.Application.Queries;

/// <summary>
/// Запрос текущего баланса игрока.
/// </summary>
/// <param name="PlayerId">Идентификатор игрока.</param>
public sealed record GetPlayerBalanceQuery(Guid PlayerId) : IQuery<decimal>;
