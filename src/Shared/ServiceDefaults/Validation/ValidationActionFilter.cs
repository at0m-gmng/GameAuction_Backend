using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GameBackend.ServiceDefaults.Validation;

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
    public ValidationActionFilter(IServiceProvider services) => _services = services;

    /// <summary>
    /// Проверяет каждый аргумент; при ошибках короткозамыкает конвейер ответом 400 ValidationProblem.
    /// </summary>
    /// <param name="context">Контекст выполняемого действия.</param>
    /// <param name="next">Делегат следующего шага конвейера.</param>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (object? argument in context.ActionArguments.Values)
        {
            if (argument is not null && _services.GetService(typeof(IValidator<>).MakeGenericType(argument.GetType())) is IValidator validator)
            {
                ValidationResult result = await validator.ValidateAsync(new ValidationContext<object>(argument));
                foreach (ValidationFailure error in result.Errors)
                {
                    context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
            }
        }

        if (context.ModelState.ErrorCount > 0)
        {
            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState));
        }
        else
        {
            await next();
        }
    }
}
