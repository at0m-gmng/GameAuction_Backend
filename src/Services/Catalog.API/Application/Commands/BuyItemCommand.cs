using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Команда покупки предмета в инвентарь игрока.
/// </summary>
/// <param name="PlayerId">Идентификатор игрока (из JWT).</param>
/// <param name="ItemId">Идентификатор предмета каталога.</param>
/// <param name="Quantity">Количество (по умолчанию 1).</param>
public sealed record BuyItemCommand(Guid PlayerId, Guid ItemId, int Quantity) : ICommand;