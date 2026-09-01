namespace GameBackend.SharedKernel.Application;

/// <summary>Обработчик запроса.</summary>
/// <typeparam name="TQuery">Тип запроса.</typeparam>
/// <typeparam name="TResult">Тип результата.</typeparam>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    /// <summary>Обрабатывает запрос и возвращает результат.</summary>
    /// <param name="query">Запрос.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
}