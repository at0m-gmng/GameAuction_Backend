using System.Security.Cryptography;
using System.Text;

namespace GameBackend.Services.Identity.API.Infrastructure.Security;

/// <summary>
/// Хеширование паролей с солью через PBKDF2.
/// Встроен в .NET, не требует внешних пакетов.
/// </summary>
public sealed class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    /// <summary>
    /// Хеширует пароль с автоматической генерацией соли.
    /// </summary>
    /// <param name="password">Пароль в открытом виде.</param>
    /// <returns>Строка формата "base64(salt).base64(hash)".</returns>
    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            Algorithm,
            HashSize);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Проверяет соответствие пароля хешу.
    /// </summary>
    /// <param name="password">Пароль для проверки.</param>
    /// <param name="storedHash">Сохранённый хеш из БД.</param>
    /// <returns>True, если пароль верный.</returns>
    public bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 2)
            return false;

        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);

        var testHash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            Algorithm,
            HashSize);

        return CryptographicOperations.FixedTimeEquals(testHash, hash);
    }
}