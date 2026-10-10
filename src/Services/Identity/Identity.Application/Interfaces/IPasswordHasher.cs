namespace GameBackend.Services.Identity.Application.Interfaces;

/// <summary>
/// Хеширование и проверка паролей.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Вычисляет хеш пароля с солью.
    /// </summary>
    /// <param name="password">Пароль в открытом виде.</param>
    /// <returns>Хеш для хранения.</returns>
    string Hash(string password);

    /// <summary>
    /// Сверяет пароль с сохранённым хешем.
    /// </summary>
    /// <param name="password">Пароль в открытом виде.</param>
    /// <param name="storedHash">Сохранённый хеш.</param>
    /// <returns>true, если пароль совпадает.</returns>
    bool Verify(string password, string storedHash);
}
