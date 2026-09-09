using GameBackend.SharedKernel.Domain;
using GameBackend.Services.Lobby.API.Domain.Events;

namespace GameBackend.Services.Lobby.API.Domain;

/// <summary>
/// Агрегат лобби: комната сбора игроков и живой аукцион за предмет, объединяет матчмейкинг и торги.
/// </summary>
public sealed class LobbyAggregate : AggregateRoot
{
    /// <summary>
    /// Идентификатор предмета из каталога, который разыгрывается в этом лобби.
    /// </summary>
    public Guid ItemId { get; private set; }

    /// <summary>
    /// Снапшот названия предмета (для витрины списка лобби).
    /// </summary>
    public string ItemName { get; private set; } = default!;

    /// <summary>
    /// Снапшот ссылки на изображение предмета (для витрины списка лобби).
    /// </summary>
    public string? ItemImageUrl { get; private set; }

    /// <summary>
    /// Стартовая цена аукциона. До первой ставки отображается как текущая.
    /// </summary>
    public decimal StartingPrice { get; private set; }

    /// <summary>
    /// Текущий статус лобби.
    /// </summary>
    public LobbyStatus Status { get; private set; }

    /// <summary>
    /// Максимальное количество игроков, которые могут участвовать в лобби.
    /// </summary>
    public int MaxParticipants { get; private set; }

    private readonly List<Guid> _participants = new();

    /// <summary>
    /// IReadOnly-коллекция участников для внешнего доступа.
    /// </summary>
    public IReadOnlyCollection<Guid> Participants => _participants.AsReadOnly();

    private readonly List<Guid> _seenPlayers = new();

    /// <summary>
    /// IReadOnly-коллекция уже заходивших игроков для маппинга ORM.
    /// </summary>
    public IReadOnlyCollection<Guid> SeenPlayers => _seenPlayers.AsReadOnly();

    private readonly List<Bid> _bids = new();

    /// <summary>
    /// IReadOnly-коллекция ставок для внешнего доступа.
    /// </summary>
    public IReadOnlyCollection<Bid> Bids => _bids.AsReadOnly();

    /// <summary>
    /// Текущая максимальная ставка — null, если ставок не было; вычисляется из истории, не хранится отдельно.
    /// </summary>
    public Bid? CurrentBid => _bids.Count == 0 ? null : _bids.MaxBy(b => b.Amount);

    /// <summary>
    /// Дата и время окончания аукциона. Null в статусе Gathering ("STARTING SOON").
    /// </summary>
    public DateTime? EndsAt { get; private set; }

    /// <summary>
    /// Идентификатор победителя аукциона. Null, пока аукцион не завершён.
    /// </summary>
    public Guid? WinnerId { get; private set; }

    private LobbyAggregate(Guid itemId, string itemName, string? itemImageUrl, decimal startingPrice, int maxParticipants)
        : base(Guid.NewGuid())
    {
        ItemId = itemId;
        ItemName = itemName;
        ItemImageUrl = itemImageUrl;
        StartingPrice = startingPrice;
        MaxParticipants = maxParticipants;
        Status = LobbyStatus.Gathering;

        AddDomainEvent(new LobbyCreated(Id, ItemId, MaxParticipants));
    }

    private LobbyAggregate()
    {
    }

    /// <summary>
    /// Фабричный метод для создания нового лобби со снапшотом данных предмета.
    /// </summary>
    /// <param name="itemId">Идентификатор предмета из каталога.</param>
    /// <param name="itemName">Снапшот названия предмета.</param>
    /// <param name="itemImageUrl">Снапшот ссылки на изображение.</param>
    /// <param name="startingPrice">Стартовая цена аукциона.</param>
    /// <param name="maxParticipants">Максимальное количество участников.</param>
    /// <returns>Новое лобби в статусе Gathering.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Если maxParticipants меньше 1.</exception>
    public static LobbyAggregate Create(Guid itemId, string itemName, string? itemImageUrl, decimal startingPrice, int maxParticipants)
    {
        if (maxParticipants < 1)
            throw new ArgumentOutOfRangeException(nameof(maxParticipants), "Минимум 1 участник для аукциона");

        return new LobbyAggregate(itemId, itemName, itemImageUrl, startingPrice, maxParticipants);
    }

    /// <summary>
    /// Добавляет игрока в лобби (до и во время торгов); возвращает true только для реально нового игрока раунда.
    /// </summary>
    /// <param name="playerId">Идентификатор игрока.</param>
    /// <returns>true, если игрок впервые в этом раунде (повод продлить/стартовать таймер); иначе false.</returns>
    /// <exception cref="InvalidOperationException">Если лобби уже завершено или отменено.</exception>
    /// <exception cref="InvalidOperationException">Если лобби заполнено.</exception>
    public bool Join(Guid playerId)
    {
        if (Status != LobbyStatus.Gathering && Status != LobbyStatus.Bidding)
            throw new InvalidOperationException("Присоединиться можно, только пока лобби открыто");

        // NOTE: повторный вход того, кто уже в лобби — идемпотентный no-op, время не трогаем.
        if (_participants.Contains(playerId))
            return false;

        if (_participants.Count >= MaxParticipants)
            throw new InvalidOperationException("Лобби заполнено");

        _participants.Add(playerId);
        AddDomainEvent(new PlayerJoinedLobby(Id, playerId, _participants.Count));

        // NOTE: +60 сек — только за нового игрока раунда; кто уже заходил (и вышел), время не двигает.
        if (_seenPlayers.Contains(playerId))
            return false;

        _seenPlayers.Add(playerId);
        return true;
    }

    /// <summary>
    /// Убирает игрока из лобби, освобождая слот; сделавший ставку остаётся участником до конца раунда.
    /// </summary>
    /// <param name="playerId">Идентификатор игрока.</param>
    public void Leave(Guid playerId)
    {
        // NOTE: сделавший ставку закреплён за раундом — уход со страницы не снимает его с аукциона.
        if (_bids.Any(b => b.PlayerId == playerId))
            return;

        if (!_participants.Remove(playerId))
            return;

        AddDomainEvent(new PlayerLeftLobby(Id, playerId, _participants.Count));

        // NOTE: последний ушёл без ставок — схлопываем раунд в Gathering, чтобы не висел активный таймер.
        if (_participants.Count == 0 && Status == LobbyStatus.Bidding && CurrentBid is null)
            ExpireWithoutBids();
    }

    /// <summary>
    /// Запускает аукцион. Переводит лобби в статус Bidding и устанавливает таймер.
    /// </summary>
    /// <param name="duration">Длительность аукциона.</param>
    /// <exception cref="InvalidOperationException">Если лобби не в статусе Gathering.</exception>
    /// <exception cref="InvalidOperationException">Если недостаточно участников.</exception>
    public void StartAuction(TimeSpan duration)
    {
        if (Status != LobbyStatus.Gathering)
            throw new InvalidOperationException("Аукцион можно запустить только в статусе Gathering");

        if (_participants.Count < 1)
            throw new InvalidOperationException("Недостаточно участников для старта аукциона");

        Status = LobbyStatus.Bidding;
        EndsAt = DateTime.UtcNow.Add(duration);
    }

    /// <summary>
    /// Продлевает уже идущий аукцион при присоединении нового игрока — время добавляется к оставшемуся.
    /// </summary>
    /// <param name="extension">Насколько продлить.</param>
    /// <exception cref="InvalidOperationException">Если лобби не в статусе Bidding.</exception>
    public void ExtendOnJoin(TimeSpan extension)
    {
        if (Status != LobbyStatus.Bidding)
            throw new InvalidOperationException("Продлить можно только активный аукцион");

        EndsAt = (EndsAt ?? DateTime.UtcNow).Add(extension);
    }

    /// <summary>
    /// Регистрирует ставку (первая не ниже стартовой, остальные — выше текущей) и продлевает таймер.
    /// </summary>
    /// <param name="playerId">Идентификатор игрока, делающего ставку.</param>
    /// <param name="amount">Сумма ставки.</param>
    /// <param name="availableBalance">Доступный баланс игрока — ставка не может его превышать.</param>
    /// <param name="extensionWindow">На сколько продлевается таймер после успешной ставки.</param>
    /// <exception cref="InvalidOperationException">Если аукцион не в статусе Bidding.</exception>
    /// <exception cref="InvalidOperationException">Если время аукциона истекло.</exception>
    /// <exception cref="InvalidOperationException">Если игрока нет в лобби.</exception>
    /// <exception cref="InvalidOperationException">Если ставка некорректна.</exception>
    public void PlaceBid(Guid playerId, decimal amount, decimal availableBalance, TimeSpan extensionWindow)
    {
        if (Status != LobbyStatus.Bidding)
            throw new InvalidOperationException("Ставки можно делать только в статусе Bidding");

        if (EndsAt.HasValue && DateTime.UtcNow >= EndsAt.Value)
            throw new InvalidOperationException("Время аукциона истекло");

        if (!_participants.Contains(playerId))
            throw new InvalidOperationException("Игрок не является участником лобби");

        if (CurrentBid is null && amount < StartingPrice)
            throw new InvalidOperationException($"Первая ставка не может быть ниже стартовой ({StartingPrice})");

        if (CurrentBid is not null && amount <= CurrentBid.Amount)
            throw new InvalidOperationException($"Ставка должна быть больше текущей ({CurrentBid.Amount})");

        if (amount > availableBalance)
            throw new InvalidOperationException($"Ставка превышает доступный баланс ({availableBalance})");

        var bid = new Bid(playerId, amount, DateTime.UtcNow);
        _bids.Add(bid);
        EndsAt = (EndsAt ?? DateTime.UtcNow).Add(extensionWindow);

        AddDomainEvent(new BidPlaced(Id, playerId, amount));
    }

    /// <summary>
    /// Завершает аукцион и определяет победителя (автора максимальной ставки).
    /// </summary>
    /// <exception cref="InvalidOperationException">Если лобби не в статусе Bidding.</exception>
    public void Complete()
    {
        if (Status != LobbyStatus.Bidding)
            throw new InvalidOperationException("Завершить можно только активный аукцион");

        Status = LobbyStatus.Completed;
        WinnerId = CurrentBid?.PlayerId;

        AddDomainEvent(new AuctionCompleted(Id, WinnerId, CurrentBid?.Amount));
    }

    /// <summary>
    /// Сбрасывает раунд без ставок обратно в Gathering — участники выкидываются, предмет остаётся на продаже.
    /// </summary>
    /// <exception cref="InvalidOperationException">Если лобби не в статусе Bidding или ставка уже была.</exception>
    public void ExpireWithoutBids()
    {
        if (Status != LobbyStatus.Bidding)
            throw new InvalidOperationException("Сбросить можно только активный аукцион");

        if (CurrentBid is not null)
            throw new InvalidOperationException("Нельзя сбросить раунд, в котором уже есть ставки");

        Status = LobbyStatus.Gathering;
        EndsAt = null;
        _participants.Clear();
        _seenPlayers.Clear();

        AddDomainEvent(new RoundExpiredWithoutBids(Id));
    }
}