using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Команда запуска аукциона по выставленному предмету — создаёт лобби или присоединяет к существующему.
/// </summary>
/// <param name="ItemId">Идентификатор предмета каталога.</param>
public sealed record StartAuctionCommand(Guid ItemId) : ICommand<Guid>;
