using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GameBackend.Services.Catalog.API.Infrastructure.Validation;

/// <summary>
/// Валидирует аргументы действия зарегистрированными FluentValidation-валидаторами до входа в контроллер.
/// </summary>
public sealed class ValidationActionFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _services;

    /// <summary>
    /// Инициализирует фильтр провайдером сервисов для резолва валидаторов.
    /// </summary>
    /// <param name="services">Провайдер сервисов.</param>
    public ValidationActionFilter(IServiceProvider services)
    {
        _services = services;
    }

    /// <summary>
    /// Проверяет каждый аргумент; при ошибках короткозамыкает конвейер ответом 400 ValidationProblem.
    /// </summary>
    /// <param name="context">Контекст выполняемого действия.</param>
    /// <param name="next">Делегат следующего шага конвейера.</param>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (_services.GetService(validatorType) is not IValidator validator)
                continue;

            var result = await validator.ValidateAsync(new ValidationContext<object>(argument));
            if (result.IsValid)
                continue;

            foreach (var error in result.Errors)
                context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState));
            return;
        }

        await next();
    }
}
