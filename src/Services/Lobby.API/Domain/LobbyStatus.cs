namespace GameBackend.Services.Lobby.API.Domain;

/// <summary>
/// Статусы лобби (smallint, шаг 100) — никогда не менять присвоенные значения, только добавлять.
/// </summary>
public enum LobbyStatus : short
{
    /// <summary>Лобби открыто для набора игроков.</summary>
    Gathering = 100,

    /// <summary>Идёт живой аукцион.</summary>
    Bidding = 200,

    /// <summary>Аукцион завершён.</summary>
    Completed = 300,

    /// <summary>Лобби отменено.</summary>
    Cancelled = 400
}