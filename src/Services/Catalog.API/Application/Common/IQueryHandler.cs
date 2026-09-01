namespace GameBackend.Services.Catalog.API.Application.Common;

/// <summary>
/// Интерфейс обработчика запроса.
/// </summary>
/// <typeparam name="TQuery">Тип запроса для обработки.</typeparam>
/// <typeparam name="TResult">Тип результата выполнения запроса.</typeparam>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Обрабатывает запрос и возвращает результат.
    /// </summary>
    /// <param name="query">Запрос для обработки.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Результат выполнения запроса.</returns>
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken);
}