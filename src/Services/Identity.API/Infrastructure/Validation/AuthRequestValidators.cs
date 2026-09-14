using FluentValidation;
using GameBackend.Services.Identity.API.Controllers.Dtos;

namespace GameBackend.Services.Identity.API.Infrastructure.Validation;

/// <summary>
/// Правила валидации запроса регистрации.
/// </summary>
public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    /// <summary>
    /// Задаёт правила: непустой ник до 50 символов, корректный email, пароль от 8 символов.
    /// </summary>
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Nickname).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

/// <summary>
/// Правила валидации запроса входа.
/// </summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    /// <summary>
    /// Задаёт правила: корректный email и непустой пароль.
    /// </summary>
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
