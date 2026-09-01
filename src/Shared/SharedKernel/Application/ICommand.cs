namespace GameBackend.SharedKernel.Application;

/// <summary>Маркер команды без результата.</summary>
public interface ICommand
{
}

/// <summary>Маркер команды с результатом.</summary>
/// <typeparam name="TResult">Тип результата.</typeparam>
public interface ICommand<TResult>
{
}