using Microsoft.AspNetCore.SignalR;

namespace GameBackend.Services.Lobby.API.Hubs;

/// <summary>
/// Сопоставляет SignalR-соединение с игроком по claim "sub" — проект не мапит его в NameIdentifier.
/// </summary>
public sealed class SubjectUserIdProvider : IUserIdProvider
{
    /// <summary>
    /// Возвращает идентификатор игрока (claim "sub") для адресной доставки через Clients.User.
    /// </summary>
    /// <param name="connection">Контекст подключения хаба.</param>
    /// <returns>Идентификатор игрока или null для неаутентифицированного соединения.</returns>
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst("sub")?.Value;
    }
}
