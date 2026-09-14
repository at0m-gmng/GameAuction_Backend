using FluentValidation;
using GameBackend.Services.Lobby.API.Controllers;

namespace GameBackend.Services.Lobby.API.Infrastructure.Validation;

/// <summary>
/// Правила валидации запроса ставки.
/// </summary>
public sealed class PlaceBidRequestValidator : AbstractValidator<PlaceBidRequest>
{
    /// <summary>
    /// Задаёт правило: сумма ставки больше нуля.
    /// </summary>
    public PlaceBidRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
