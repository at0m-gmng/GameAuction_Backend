using GameBackend.Services.Generation.API.Application.Interfaces;
using GameBackend.Services.Generation.API.Domain;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Generation.API.Application.Commands;

/// <summary>
/// Загружает пулы из БД и вызывает генератор.
/// </summary>
public sealed class GenerateItemCommandHandler : ICommandHandler<GenerateItemCommand, GeneratedItemDto>
{
    private readonly IGenerationPoolRepository _repository;
    private readonly IItemGenerator _generator;

    /// <summary>
    /// Инициализирует хендлер репозиторием и генератором.
    /// </summary>
    /// <param name="repository">Репозиторий пулов генерации.</param>
    /// <param name="generator">Генератор заготовок.</param>
    public GenerateItemCommandHandler(IGenerationPoolRepository repository, IItemGenerator generator)
    {
        _repository = repository;
        _generator = generator;
    }

    /// <summary>
    /// Загружает архетипы и уровни редкости, возвращает сгенерированную заготовку.
    /// </summary>
    /// <param name="command">Команда генерации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task<GeneratedItemDto> Handle(GenerateItemCommand command, CancellationToken cancellationToken)
    {
        var archetypes = await _repository.GetArchetypesAsync(cancellationToken);
        var tiers = await _repository.GetRarityTiersAsync(cancellationToken);

        return _generator.Create(archetypes, tiers);
    }
}