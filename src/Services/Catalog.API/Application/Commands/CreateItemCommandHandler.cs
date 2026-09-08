using GameBackend.SharedKernel.Application;
using GameBackend.Services.Catalog.API.Application.Interfaces;
using GameBackend.Services.Catalog.API.Domain;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Обработчик команды создания предмета: создаёт агрегат через фабричный метод и сохраняет его.
/// </summary>
public sealed class CreateItemCommandHandler : ICommandHandler<CreateItemCommand, Guid>
{
    /// <summary>
    /// Репозиторий для сохранения предметов.
    /// </summary>
    private readonly IItemRepository _repository;

    /// <summary>
    /// Инициализирует обработчик с указанным репозиторием.
    /// </summary>
    /// <param name="repository">Репозиторий предметов.</param>
    public CreateItemCommandHandler(IItemRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Обрабатывает команду: создаёт предмет и сохраняет его.
    /// </summary>
    /// <param name="command">Команда создания предмета.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Идентификатор созданного предмета.</returns>
    public async Task<Guid> Handle(CreateItemCommand command, CancellationToken cancellationToken)
    {
        var item = Item.Create(
            command.Name,
            command.Description,
            command.Category,
            command.Rarity,
            command.ImageUrl,
            command.StartingPrice,
            command.Stock);

        await _repository.SaveAsync(item, cancellationToken);

        return item.Id;
    }
}