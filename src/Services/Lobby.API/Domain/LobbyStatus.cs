namespace GameBackend.Services.Lobby.API.Domain;

/// <summary>
/// Статусы жизненного цикла лобби.
/// Хранится как smallint. Шаг 100 — запас для вставки промежуточных статусов.
/// Правило: никогда не менять присвоенные значения, только добавлять новые.
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