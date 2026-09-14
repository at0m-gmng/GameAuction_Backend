using FluentValidation;
using GameBackend.Services.Catalog.API.Controllers;

namespace GameBackend.Services.Catalog.API.Infrastructure.Validation;

/// <summary>
/// Правила валидации запроса покупки предмета.
/// </summary>
public sealed class BuyItemRequestValidator : AbstractValidator<BuyItemRequest>
{
    /// <summary>
    /// Задаёт правила: указан предмет и количество больше нуля.
    /// </summary>
    public BuyItemRequestValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

/// <summary>
/// Правила валидации запроса на выставление предмета на продажу.
/// </summary>
public sealed class ListForSaleRequestValidator : AbstractValidator<ListForSaleRequest>
{
    /// <summary>
    /// Задаёт правило: стартовая цена больше нуля.
    /// </summary>
    public ListForSaleRequestValidator()
    {
        RuleFor(x => x.StartingPrice).GreaterThan(0);
    }
}
