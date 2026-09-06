using GameBackend.Services.Generation.API.Application.Commands;
using GameBackend.Services.Generation.API.Domain;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace GameBackend.Services.Generation.API.Controllers;

/// <summary>
/// Внутренний эндпоинт генерации заготовок. Принимает только сервисы с общим секретом.
/// </summary>
[ApiController]
[Route("api/generation")]
public sealed class GenerationController : ControllerBase
{
    private readonly GenerateItemCommandHandler _generate;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Инициализирует контроллер хендлером и конфигурацией.
    /// </summary>
    /// <param name="generate">Хендлер генерации.</param>
    /// <param name="configuration">Конфигурация для внутреннего ключа.</param>
    public GenerationController(GenerateItemCommandHandler generate, IConfiguration configuration)
    {
        _generate = generate;
        _configuration = configuration;
    }

    /// <summary>
    /// Генерирует одну заготовку предмета.
    /// </summary>
    /// <param name="ct">Токен отмены.</param>
    [HttpPost("generate")]
    public async Task<ActionResult<GeneratedItemDto>> Generate(CancellationToken ct = default)
    {
        if (!IsInternalCaller())
            return Unauthorized();

        try
        {
            var result = await _generate.Handle(new GenerateItemCommand(), ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Сверяет X-Internal-Key с общим секретом в постоянное время.
    /// </summary>
    private bool IsInternalCaller()
    {
        var expected = _configuration["InternalApi:Key"];
        var actual = Request.Headers["X-Internal-Key"].ToString();

        if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(actual))
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(actual),
            Encoding.UTF8.GetBytes(expected));
    }
}