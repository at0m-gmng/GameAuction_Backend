using GameBackend.SharedKernel.Application;
using GameBackend.Services.Lobby.API.Application.Interfaces;
using GameBackend.Services.Lobby.API.Infrastructure.ExternalServices;

namespace GameBackend.Services.Lobby.API.Application.Commands;

/// <summary>
/// Обработчик команды ставки.
/// </summary>
public sealed class PlaceBidCommandHandler : ICommandHandler<PlaceBidCommand>
{
    private static readonly TimeSpan BidExtensionWindow = TimeSpan.FromSeconds(15);

    private readonly ILobbyRepository _repository;
    private readonly IIdentityServiceClient _identityClient;

    /// <summary>
    /// Инициализирует обработчик зависимостями.
    /// </summary>
    /// <param name="repository">Репозиторий лобби.</param>
    /// <param name="identityClient">Клиент к Identity.API (проверка баланса).</param>
    public PlaceBidCommandHandler(ILobbyRepository repository, IIdentityServiceClient identityClient)
    {
        _repository = repository;
        _identityClient = identityClient;
    }

    /// <summary>
    /// Загружает лобби и баланс игрока, регистрирует ставку и сохраняет.
    /// </summary>
    /// <param name="command">Команда.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task Handle(PlaceBidCommand command, CancellationToken cancellationToken)
    {
        var lobby = await _repository.GetByIdAsync(command.LobbyId, cancellationToken)
                    ?? throw new InvalidOperationException($"Лобби {command.LobbyId} не найдено");

        decimal balance;
        try
        {
            balance = await _identityClient.GetBalanceAsync(command.PlayerId, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            // NOTE: без проверки баланса ставку принимать нельзя — сетевой сбой превращаем в 400, а не голый 500.
            throw new InvalidOperationException("Не удалось проверить баланс — попробуйте ещё раз", ex);
        }

        lobby.PlaceBid(command.PlayerId, command.Amount, balance, BidExtensionWindow);

        await _repository.SaveAsync(lobby, cancellationToken);
    }
}