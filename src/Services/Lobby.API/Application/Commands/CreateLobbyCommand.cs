using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Lobby.API.Application.Commands;

/// <summary>
/// Команда создания лобби со снапшотом витринных данных предмета; возвращает идентификатор лобби.
/// </summary>
/// <param name="ItemId">Идентификатор предмета.</param>
/// <param name="ItemName">Снапшот названия предмета.</param>
/// <param name="ItemImageUrl">Снапшот ссылки на изображение.</param>
/// <param name="StartingPrice">Стартовая цена аукциона.</param>
/// <param name="MaxParticipants">Максимальное количество участников.</param>
public sealed record CreateLobbyCommand(
    Guid ItemId,
    string ItemName,
    string? ItemImageUrl,
    decimal StartingPrice,
    int MaxParticipants) : ICommand<Guid>;