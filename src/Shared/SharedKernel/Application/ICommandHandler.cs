namespace GameBackend.SharedKernel.Application;

/// <summary>Обработчик команды без результата.</summary>
/// <typeparam name="TCommand">Тип команды.</typeparam>
public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    /// <summary>Обрабатывает команду.</summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task Handle(TCommand command, CancellationToken cancellationToken);
}

/// <summary>Обработчик команды с результатом.</summary>
/// <typeparam name="TCommand">Тип команды.</typeparam>
/// <typeparam name="TResult">Тип результата.</typeparam>
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    /// <summary>Обрабатывает команду и возвращает результат.</summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}