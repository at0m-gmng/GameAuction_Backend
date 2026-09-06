using System.Security.Cryptography;
using System.Text;

namespace GameBackend.SharedKernel.Security;

/// <summary>
/// Сравнивает присланный ключ с ожидаемым в постоянное время.
/// </summary>
public sealed class InternalCallerValidator : IInternalCallerValidator
{
    private readonly string? _expectedKey;

    public InternalCallerValidator(string? expectedKey)
    {
        _expectedKey = expectedKey;
    }

    public bool IsValid(string? providedKey)
    {
        if (string.IsNullOrEmpty(_expectedKey) || string.IsNullOrEmpty(providedKey))
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(providedKey),
            Encoding.UTF8.GetBytes(_expectedKey));
    }
}
