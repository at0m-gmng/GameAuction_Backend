namespace GameBackend.Services.Catalog.API.Application.Common;

/// <summary>
/// Маркерный интерфейс для команд, которые не возвращают результат.
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Маркерный интерфейс для команд, которые возвращают результат.
/// </summary>
/// <typeparam name="TResult">Тип результата выполнения команды.</typeparam>
public interface ICommand<TResult>
{
}