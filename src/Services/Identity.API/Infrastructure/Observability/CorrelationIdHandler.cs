namespace GameBackend.Services.Identity.API.Infrastructure.Observability;

/// <summary>
/// Пробрасывает correlation-id текущего запроса в исходящие межсервисные HTTP-вызовы.
/// </summary>
public sealed class CorrelationIdHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;

    /// <summary>
    /// Инициализирует обработчик доступом к текущему HTTP-контексту.
    /// </summary>
    /// <param name="accessor">Аксессор HTTP-контекста.</param>
    public CorrelationIdHandler(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    /// <summary>
    /// Добавляет заголовок correlation-id к исходящему запросу, если он ещё не задан.
    /// </summary>
    /// <param name="request">Исходящий запрос.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Ответ вызванного сервиса.</returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_accessor.HttpContext?.Items[CorrelationIdMiddleware.HeaderName] is string correlationId
            && !request.Headers.Contains(CorrelationIdMiddleware.HeaderName))
        {
            request.Headers.Add(CorrelationIdMiddleware.HeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
