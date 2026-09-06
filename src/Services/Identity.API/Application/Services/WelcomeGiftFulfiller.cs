using GameBackend.Services.Identity.API.Application.Interfaces;
using GameBackend.Services.Identity.API.Domain;
using GameBackend.Services.Identity.API.Infrastructure.ExternalServices;

namespace GameBackend.Services.Identity.API.Application.Services;

/// <summary>
/// Выдаёт игроку приветственный подарочный предмет, если он ещё не выдан.
/// Вызывается и при регистрации, и при входе — если Generation.API или
/// Catalog.API были недоступны при регистрации, подарок довыдаётся при
/// следующем успешном входе.
/// </summary>
public sealed class WelcomeGiftFulfiller
{
    private readonly IGenerationServiceClient _generationClient;
    private readonly ICatalogServiceClient _catalogClient;
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<WelcomeGiftFulfiller> _logger;

    public WelcomeGiftFulfiller(
        IGenerationServiceClient generationClient,
        ICatalogServiceClient catalogClient,
        IPlayerRepository playerRepository,
        ILogger<WelcomeGiftFulfiller> logger)
    {
        _generationClient = generationClient;
        _catalogClient = catalogClient;
        _playerRepository = playerRepository;
        _logger = logger;
    }

    /// <summary>
    /// Пытается выдать подарок, если он ещё не выдан. Никогда не бросает
    /// исключение наружу — выдача подарка не должна ломать регистрацию/вход.
    /// </summary>
    /// <param name="player">Игрок-получатель.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task EnsureGrantedAsync(Player player, CancellationToken cancellationToken)
    {
        if (player.WelcomeGiftGranted)
            return;

        try
        {
            var generatedItem = await _generationClient.GenerateAsync(cancellationToken);
            await _catalogClient.GrantItemAsync(player.Id, generatedItem, cancellationToken);

            player.MarkWelcomeGiftGranted();
            await _playerRepository.SaveAsync(player, cancellationToken);
        }
        catch (Exception ex)
        {
            // NOTE: best-effort — Generation.API/Catalog.API недоступны, значит
            // просто пробуем ещё раз при следующем входе (см. WelcomeGiftGranted).
            _logger.LogWarning(ex, "Не удалось выдать приветственный подарок игроку {PlayerId}", player.Id);
        }
    }
}
