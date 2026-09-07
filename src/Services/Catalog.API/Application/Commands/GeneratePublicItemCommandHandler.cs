using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Domain;
using GameBackend.Services.Catalog.API.Infrastructure.ExternalServices;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Обработчик генерации публичного предмета: запрашивает заготовку у Generation.API
/// и сохраняет её как карточку каталога (остаток — 1, без владельца).
/// </summary>
public sealed class GeneratePublicItemCommandHandler : ICommandHandler<GeneratePublicItemCommand, Guid>
{
    private const int GeneratedItemStock = 1;

    private readonly IGenerationServiceClient _generationClient;
    private readonly IItemRepository _itemRepository;

    /// <summary>
    /// Инициализирует обработчик клиентом генерации и репозиторием предметов.
    /// </summary>
    /// <param name="generationClient">Клиент к Generation.API.</param>
    /// <param name="itemRepository">Репозиторий предметов каталога.</param>
    public GeneratePublicItemCommandHandler(IGenerationServiceClient generationClient, IItemRepository itemRepository)
    {
        _generationClient = generationClient;
        _itemRepository = itemRepository;
    }

    public async Task<Guid> Handle(GeneratePublicItemCommand command, CancellationToken cancellationToken)
    {
        var generated = await _generationClient.GenerateAsync(cancellationToken);

        var item = Item.Create(
            generated.Name,
            generated.Description,
            generated.Category,
            generated.Rarity,
            generated.ImageUrl,
            generated.StartingPrice,
            GeneratedItemStock);

        await _itemRepository.SaveAsync(item, cancellationToken);

        return item.Id;
    }
}
