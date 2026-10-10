namespace GameBackend.SharedKernel.Application.Persistence;

/// <summary>
/// Запись журнала идемпотентности.
/// </summary>
/// <param name="Result">Сохранённый результат операции; null, если результата нет.</param>
public sealed record ProcessedOperationRecord(string? Result);
