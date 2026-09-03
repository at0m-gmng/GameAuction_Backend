using GameBackend.Services.Identity.API.Application.Commands;
using GameBackend.Services.Identity.API.Controllers.Dtos;
using GameBackend.SharedKernel.Application;
using Microsoft.AspNetCore.Mvc;

namespace GameBackend.Services.Identity.API.Controllers;

/// <summary>
/// Контроллер аутентификации: регистрация и вход игроков.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ICommandHandler<RegisterCommand, string> _registerHandler;
    private readonly ICommandHandler<LoginCommand, string> _loginHandler;

    /// <summary>
    /// Инициализирует контроллер хендлерами команд.
    /// </summary>
    /// <param name="registerHandler">Хендлер регистрации.</param>
    /// <param name="loginHandler">Хендлер входа.</param>
    public AuthController(
        ICommandHandler<RegisterCommand, string> registerHandler,
        ICommandHandler<LoginCommand, string> loginHandler)
    {
        _registerHandler = registerHandler;
        _loginHandler = loginHandler;
    }

    /// <summary>
    /// Регистрирует нового игрока и возвращает JWT-токен.
    /// </summary>
    /// <param name="request">Данные регистрации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>JWT-токен.</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(request.Nickname, request.Email, request.Password);
        var token = await _registerHandler.Handle(command, cancellationToken);
        return Ok(new AuthResponse(token));
    }

    /// <summary>
    /// Проверяет учётные данные и возвращает JWT-токен.
    /// </summary>
    /// <param name="request">Данные входа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>JWT-токен.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var token = await _loginHandler.Handle(command, cancellationToken);
        return Ok(new AuthResponse(token));
    }
}