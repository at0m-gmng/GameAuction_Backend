using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Команда выдачи предмета победителю аукциона — только в инвентарь, без списания склада каталога.
/// </summary>
/// <param name="PlayerId">Идентификатор победителя.</param>
/// <param name="ItemId">Идентификатор предмета каталога.</param>
/// <param name="Quantity">Количество копий (по умолчанию 1).</param>
/// <param name="Price">Цена продажи — новая цена предмета в каталоге.</param>
public sealed record AwardItemCommand(Guid PlayerId, Guid ItemId, int Quantity, decimal Price) : ICommand;
