namespace GameBackend.Services.Catalog.API.Application.Common;

/// <summary>
/// Маркерный интерфейс для запросов, которые возвращают результат.
/// Запросы используются только для чтения данных и не изменяют состояние системы.
/// </summary>
/// <typeparam name="TResult">Тип результата выполнения запроса.</typeparam>
public interface IQuery<TResult>
{
}