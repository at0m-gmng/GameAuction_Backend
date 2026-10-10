using GameBackend.Services.Generation.Domain;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Generation.Application.Commands;

/// <summary>
/// Команда генерации одной заготовки предмета.
/// </summary>
public sealed record GenerateItemCommand : ICommand<GeneratedItemDto>;