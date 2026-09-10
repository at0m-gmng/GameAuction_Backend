using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Команда снятия предмета с продажи — возвращает его в инвентарь владельца.
/// </summary>
/// <param name="PlayerId">Идентификатор игрока-владельца.</param>
/// <param name="ItemId">Идентификатор предмета.</param>
public sealed record UnlistItemCommand(Guid PlayerId, Guid ItemId) : ICommand;
