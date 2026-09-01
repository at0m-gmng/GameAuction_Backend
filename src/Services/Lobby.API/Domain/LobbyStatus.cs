namespace GameBackend.Services.Lobby.API.Domain;

/// <summary>
/// Статусы жизненного цикла лобби.
/// </summary>
public enum LobbyStatus
{
    /// <summary>
    /// Лобби открыто для набора игроков.
    /// </summary>
    Gathering = 0,

    /// <summary>
    /// Идёт живой аукцион, игроки делают ставки.
    /// </summary>
    Bidding = 1,

    /// <summary>
    /// Аукцион завершён, определён победитель.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Лобби отменено (недостаточно игроков или другая причина).
    /// </summary>
    Cancelled = 3
}