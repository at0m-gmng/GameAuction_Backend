using GameBackend.SharedKernel.Domain;

namespace GameBackend.Services.Lobby.API.Domain;

/// <summary>
/// Ставка игрока в рамках аукциона внутри лобби.
/// Является частью агрегата Lobby, не может существовать отдельно.
/// </summary>
public sealed class Bid : Entity
{
    /// <summary>
    /// Идентификатор игрока, сделавшего ставку.
    /// </summary>
    public Guid PlayerId { get; private set; }

    /// <summary>
    /// Сумма ставки.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Дата и время, когда была сделана ставка.
    /// </summary>
    public DateTime PlacedAt { get; private set; }

    /// <summary>
    /// Инициализирует новую ставку.
    /// </summary>
    /// <param name="playerId">Идентификатор игрока.</param>
    /// <param name="amount">Сумма ставки (должна быть больше нуля).</param>
    /// <param name="placedAt">Время ставки.</param>
    public Bid(Guid playerId, decimal amount, DateTime placedAt)
        : base(Guid.NewGuid())
    {
        PlayerId = playerId;
        Amount = amount;
        PlacedAt = placedAt;
    }

    /// <summary>
    /// Приватный конструктор для поддержки ORM.
    /// </summary>
    private Bid()
    {
    }
}