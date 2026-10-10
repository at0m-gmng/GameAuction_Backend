using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace GameBackend.ServiceDefaults.Observability;

/// <summary>
/// Присваивает запросу correlation-id (из заголовка или новый) и обогащает им логи и ответ.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    /// <summary>
    /// Имя заголовка сквозного идентификатора запроса.
    /// </summary>
    public const string HeaderName = "X-Correlation-ID";

    private const string LogPropertyName = "CorrelationId";

    private readonly RequestDelegate _next;

    /// <summary>
    /// Инициализирует middleware следующим шагом конвейера.
    /// </summary>
    /// <param name="next">Следующий шаг конвейера.</param>
    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    /// <summary>
    /// Достаёт или генерирует correlation-id, кладёт в лог-контекст, ответ и Items запроса.
    /// </summary>
    /// <param name="context">Контекст HTTP-запроса.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId = context.Request.Headers.TryGetValue(HeaderName, out StringValues incoming) && !StringValues.IsNullOrEmpty(incoming)
            ? incoming.ToString()
            : Guid.NewGuid().ToString();

        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty(LogPropertyName, correlationId))
        {
            await _next(context);
        }
    }
}
