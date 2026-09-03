using GameBackend.SharedKernel.Domain;
using GameBackend.Services.Lobby.API.Domain.Events;

namespace GameBackend.Services.Lobby.API.Domain;

/// <summary>
/// Агрегат, представляющий игровое лобби — комнату, где собирается группа игроков
/// и проходит живой аукцион за предмет. Объединяет матчмейкинг и аукцион в единый поток.
/// Хранит денормализованный снапшот витринных данных предмета для быстрых чтений.
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

    /// <summary>
    /// Список игроков, зарегистрировавшихся в лобби.
    /// </summary>
    private readonly List<Guid> _participants = new();

    /// <summary>
    /// IReadOnly-коллекция участников для внешнего доступа.
    /// </summary>
    public IReadOnlyCollection<Guid> Participants => _participants.AsReadOnly();

    /// <summary>
    /// История ставок в аукционе.
    /// </summary>
    private readonly List<Bid> _bids = new();

    /// <summary>
    /// IReadOnly-коллекция ставок для внешнего доступа.
    /// </summary>
    public IReadOnlyCollection<Bid> Bids => _bids.AsReadOnly();

    /// <summary>
    /// Текущая максимальная ставка. Null, если ставок ещё не было.
    /// Вычисляется из истории ставок, не хранится отдельно.
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

    /// <summary>
    /// Инициализирует новое лобби. Используйте фабричный метод Create.
    /// </summary>
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

    /// <summary>
    /// Приватный конструктор для поддержки ORM.
    /// </summary>
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
    /// <exception cref="ArgumentOutOfRangeException">Если maxParticipants меньше 2.</exception>
    public static LobbyAggregate Create(Guid itemId, string itemName, string? itemImageUrl, decimal startingPrice, int maxParticipants)
    {
        if (maxParticipants < 2)
            throw new ArgumentOutOfRangeException(nameof(maxParticipants), "Минимум 2 участника для аукциона");

        return new LobbyAggregate(itemId, itemName, itemImageUrl, startingPrice, maxParticipants);
    }

    /// <summary>
    /// Добавляет игрока в лобби. Работает только в статусе Gathering и при наличии мест.
    /// </summary>
    /// <param name="playerId">Идентификатор игрока.</param>
    /// <exception cref="InvalidOperationException">Если лобби не в статусе Gathering.</exception>
    /// <exception cref="InvalidOperationException">Если лобби заполнено.</exception>
    /// <exception cref="InvalidOperationException">Если игрок уже в лобби.</exception>
    public void Join(Guid playerId)
    {
        if (Status != LobbyStatus.Gathering)
            throw new InvalidOperationException("Присоединиться можно только в статусе Gathering");

        if (_participants.Count >= MaxParticipants)
            throw new InvalidOperationException("Лобби заполнено");

        if (_participants.Contains(playerId))
            throw new InvalidOperationException("Игрок уже в лобби");

        _participants.Add(playerId);

        AddDomainEvent(new PlayerJoinedLobby(Id, playerId, _participants.Count));
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

        if (_participants.Count < 2)
            throw new InvalidOperationException("Недостаточно участников для старта аукциона");

        Status = LobbyStatus.Bidding;
        EndsAt = DateTime.UtcNow.Add(duration);
    }

    /// <summary>
    /// Регистрирует новую ставку. Первая ставка не ниже стартовой, остальные — выше текущей.
    /// </summary>
    /// <param name="playerId">Идентификатор игрока, делающего ставку.</param>
    /// <param name="amount">Сумма ставки.</param>
    /// <exception cref="InvalidOperationException">Если аукцион не в статусе Bidding.</exception>
    /// <exception cref="InvalidOperationException">Если время аукциона истекло.</exception>
    /// <exception cref="InvalidOperationException">Если игрока нет в лобби.</exception>
    /// <exception cref="InvalidOperationException">Если ставка некорректна.</exception>
    public void PlaceBid(Guid playerId, decimal amount)
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

        var bid = new Bid(playerId, amount, DateTime.UtcNow);
        _bids.Add(bid);

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
}