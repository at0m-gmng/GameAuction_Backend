using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Команда выдачи предмета победителю аукциона — только в инвентарь, без списания склада каталога.
/// </summary>
/// <param name="IdempotencyKey">Ключ идемпотентности — повтор с тем же ключом не выдаёт предмет дважды.</param>
/// <param name="PlayerId">Идентификатор победителя.</param>
/// <param name="ItemId">Идентификатор предмета каталога.</param>
/// <param name="Quantity">Количество копий (по умолчанию 1).</param>
/// <param name="Price">Цена продажи — новая цена предмета в каталоге.</param>
public sealed record AwardItemCommand(string IdempotencyKey, Guid PlayerId, Guid ItemId, int Quantity, decimal Price) : ICommand<Guid?>;

