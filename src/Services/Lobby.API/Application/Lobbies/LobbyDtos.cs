using GameBackend.Services.Lobby.API.Domain;
using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Lobby.API.Application.Lobbies;

/// <summary>
/// DTO строки списка открытых лобби (экран "Список лобби").
/// </summary>
public sealed record LobbyListDto(
    Guid Id,
    Guid ItemId,
    string ItemName,
    string? ItemImageUrl,
    decimal StartingPrice,
    LobbyStatus Status,
    int SlotsTaken,
    int MaxSlots,
    decimal CurrentBid,
    DateTime? EndsAt);

/// <summary>
/// DTO одной ставки для ленты ставок на экране лобби.
/// </summary>
public sealed record LobbyBidDto(
    Guid PlayerId,
    decimal Amount,
    DateTime PlacedAt);

/// <summary>
/// DTO детального экрана лобби: витрина, слоты, лента ставок, таймер.
/// </summary>
public sealed record LobbyDetailsDto(
    Guid Id,
    Guid ItemId,
    string ItemName,
    string? ItemImageUrl,
    ItemRarity ItemRarity,
    decimal StartingPrice,
    LobbyStatus Status,
    int MaxSlots,
    IReadOnlyCollection<Guid> Participants,
    decimal CurrentBid,
    Guid? CurrentBidderId,
    DateTime? EndsAt,
    Guid? WinnerId,
    IReadOnlyCollection<LobbyBidDto> Bids);

/// <summary>
/// DTO статистики побед и поражений игрока (экран "Профиль").
/// </summary>
public sealed record PlayerAuctionStatsDto(int Wins, int Losses);

/// <summary>
/// DTO строки истории аукционов игрока (экран "Профиль").
/// </summary>
public sealed record PlayerAuctionHistoryDto(
    Guid LobbyId,
    string ItemName,
    string? ItemImageUrl,
    ItemRarity ItemRarity,
    decimal FinalPrice,
    bool Won,
    DateTime? EndedAt);