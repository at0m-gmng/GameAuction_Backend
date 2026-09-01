using GameBackend.SharedKernel.Domain;
using GameBackend.Services.Lobby.API.Domain.Events;

namespace GameBackend.Services.Lobby.API.Domain;

/// <summary>
/// Агрегат, представляющий игровое лобби — комнату, где собирается группа игроков
/// и проходит живой аукцион за предмет. Объединяет матчмейкинг и аукцион в единый поток.
/// </summary>
public sealed class Lobby : AggregateRoot
{
    /// <summary>
    /// Идентификатор предмета из каталога, который разыгрывается в этом лобби.
    /// </summary>
    public Guid ItemId { get; private set; }

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
    /// </summary>
    public Bid? CurrentBid { get; private set; }

    /// <summary>
    /// Дата и время окончания аукциона. Актуально только в статусе Bidding.
    /// </summary>
    public DateTime? EndsAt { get; private set; }

    /// <summary>
    /// Идентификатор победителя аукциона. Null, пока аукцион не завершён.
    /// </summary>
    public Guid? WinnerId { get; private set; }

    /// <summary>
    /// Инициализирует новое лобби. Используйте фабричный метод Create.
    /// </summary>
    /// <param name="itemId">Идентификатор предмета из каталога.</param>
    /// <param name="maxParticipants">Максимальное количество участников.</param>
    private Lobby(Guid itemId, int maxParticipants)
        : base(Guid.NewGuid())
    {
        ItemId = itemId;
        MaxParticipants = maxParticipants;
        Status = LobbyStatus.Gathering;

        AddDomainEvent(new LobbyCreated(Id, ItemId, MaxParticipants));
    }

    /// <summary>
    /// Приватный конструктор для поддержки ORM.
    /// </summary>
    private Lobby()
    {
    }

    /// <summary>
    /// Фабричный метод для создания нового лобби.
    /// </summary>
    /// <param name="itemId">Идентификатор предмета из каталога.</param>
    /// <param name="maxParticipants">Максимальное количество участников.</param>
    /// <returns>Новое лобби в статусе Gathering.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Если maxParticipants меньше 2.</exception>
    public static Lobby Create(Guid itemId, int maxParticipants)
    {
        if (maxParticipants < 2)
            throw new ArgumentOutOfRangeException(nameof(maxParticipants), "Минимум 2 участника для аукциона");

        return new Lobby(itemId, maxParticipants);
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
    /// Регистрирует новую ставку. Ставка должна быть больше текущей максимальной.
    /// </summary>
    /// <param name="playerId">Идентификатор игрока, делающего ставку.</param>
    /// <param name="amount">Сумма ставки.</param>
    /// <exception cref="InvalidOperationException">Если аукцион не в статусе Bidding.</exception>
    /// <exception cref="InvalidOperationException">Если время аукциона истекло.</exception>
    /// <exception cref="InvalidOperationException">Если игрока нет в лобби.</exception>
    /// <exception cref="InvalidOperationException">Если ставка не больше текущей.</exception>
    public void PlaceBid(Guid playerId, decimal amount)
    {
        if (Status != LobbyStatus.Bidding)
            throw new InvalidOperationException("Ставки можно делать только в статусе Bidding");

        if (EndsAt.HasValue && DateTime.UtcNow >= EndsAt.Value)
            throw new InvalidOperationException("Время аукциона истекло");

        if (!_participants.Contains(playerId))
            throw new InvalidOperationException("Игрок не является участником лобби");

        if (CurrentBid is not null && amount <= CurrentBid.Amount)
            throw new InvalidOperationException($"Ставка должна быть больше текущей ({CurrentBid.Amount})");

        var bid = new Bid(playerId, amount, DateTime.UtcNow);
        _bids.Add(bid);
        CurrentBid = bid;

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