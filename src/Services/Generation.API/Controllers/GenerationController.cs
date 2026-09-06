using GameBackend.Services.Generation.API.Application.Commands;
using GameBackend.Services.Generation.API.Domain;
using GameBackend.SharedKernel.Security;
using Microsoft.AspNetCore.Mvc;

namespace GameBackend.Services.Generation.API.Controllers;

/// <summary>
/// Внутренний эндпоинт генерации заготовок. Принимает только сервисы с общим секретом.
/// </summary>
[ApiController]
[Route("api/generation")]
public sealed class GenerationController : ControllerBase
{
    private readonly GenerateItemCommandHandler _generate;
    private readonly IInternalCallerValidator _internalCallerValidator;

    /// <summary>
    /// Инициализирует контроллер хендлером и валидатором внутренних вызовов.
    /// </summary>
    /// <param name="generate">Хендлер генерации.</param>
    /// <param name="internalCallerValidator">Проверка X-Internal-Key.</param>
    public GenerationController(GenerateItemCommandHandler generate, IInternalCallerValidator internalCallerValidator)
    {
        _generate = generate;
        _internalCallerValidator = internalCallerValidator;
    }

    /// <summary>
    /// Генерирует одну заготовку предмета.
    /// </summary>
    /// <param name="ct">Токен отмены.</param>
    [HttpPost("generate")]
    public async Task<ActionResult<GeneratedItemDto>> Generate(CancellationToken ct = default)
    {
        if (!_internalCallerValidator.IsValid(Request.Headers["X-Internal-Key"]))
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
}