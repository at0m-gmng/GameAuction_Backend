using Microsoft.AspNetCore.SignalR;

namespace GameBackend.Services.Lobby.API.Hubs;

/// <summary>
/// SignalR-хаб для реального времени внутри лобби.
/// Управляет подпиской клиентов на события конкретного лобби через группы.
/// </summary>
public class LobbyHub : Hub
{
    /// <summary>
    /// Подписывает подключённого клиента на ленту событий лобби.
    /// </summary>
    /// <param name="lobbyId">Идентификатор лобби.</param>
    public async Task JoinLobby(Guid lobbyId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(lobbyId));
    }

    /// <summary>
    /// Отписывает подключённого клиента от ленты событий лобби.
    /// </summary>
    /// <param name="lobbyId">Идентификатор лобби.</param>
    public async Task LeaveLobby(Guid lobbyId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(lobbyId));
    }

    /// <summary>
    /// Формирует имя группы SignalR для лобби.
    /// </summary>
    /// <param name="lobbyId">Идентификатор лобби.</param>
    /// <returns>Имя группы.</returns>
    public static string GroupName(Guid lobbyId) => $"lobby-{lobbyId}";
}