namespace GameBackend.SharedKernel.Application.Persistence;

/// <summary>
/// Нарушение уникальности при сохранении: запись с таким ключом уже создана параллельным запросом.
/// </summary>
public sealed class DuplicateEntryException : Exception
{
    private const string DefaultMessage = "Запись с таким ключом уже существует.";

    /// <summary>
    /// Создаёт исключение с исходной ошибкой хранилища.
    /// </summary>
    /// <param name="innerException">Исходная ошибка хранилища.</param>
    public DuplicateEntryException(Exception innerException) : base(DefaultMessage, innerException)
    {
    }
}
