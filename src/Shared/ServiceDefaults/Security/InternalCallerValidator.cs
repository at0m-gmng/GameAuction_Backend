using System.Security.Cryptography;
using System.Text;

namespace GameBackend.ServiceDefaults.Security;

/// <summary>
/// Сравнивает присланный ключ с ожидаемым в постоянное время.
/// </summary>
public sealed class InternalCallerValidator : IInternalCallerValidator
{
    private readonly string? _expectedKey;

    /// <summary>
    /// Инициализирует валидатор ожидаемым ключом.
    /// </summary>
    /// <param name="expectedKey">Общий секрет сервисов.</param>
    public InternalCallerValidator(string? expectedKey) => _expectedKey = expectedKey;

    /// <inheritdoc />
    public bool IsValid(string? providedKey) =>
        !string.IsNullOrEmpty(_expectedKey)
        && !string.IsNullOrEmpty(providedKey)
        && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(providedKey), Encoding.UTF8.GetBytes(_expectedKey));
}
