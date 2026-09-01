namespace GameBackend.Services.Catalog.API.Application.Common;

/// <summary>
/// Интерфейс обработчика команды без возвращаемого результата.
/// </summary>
/// <typeparam name="TCommand">Тип команды для обработки.</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Обрабатывает команду.
    /// </summary>
    /// <param name="command">Команда для обработки.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    Task Handle(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Интерфейс обработчика команды с возвращаемым результатом.
/// </summary>
/// <typeparam name="TCommand">Тип команды для обработки.</typeparam>
/// <typeparam name="TResult">Тип результата выполнения команды.</typeparam>
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    /// <summary>
    /// Обрабатывает команду и возвращает результат.
    /// </summary>
    /// <param name="command">Команда для обработки.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат выполнения команды.</returns>
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}