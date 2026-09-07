using GameBackend.SharedKernel.Application;

namespace GameBackend.Services.Catalog.API.Application.Commands;

/// <summary>
/// Команда генерации одного публичного предмета витрины через Generation.API.
/// </summary>
public sealed record GeneratePublicItemCommand : ICommand<Guid>;
