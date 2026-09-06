using GameBackend.Services.Generation.API.Domain;
using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Generation.API.Application.Commands;

/// <summary>
/// Команда генерации одной заготовки предмета.
/// </summary>
public sealed record GenerateItemCommand : ICommand<GeneratedItemDto>;