namespace BankMore.Transfers.Domain.IdempotenciaAggregate;

public enum IdempotenciaResult : uint
{
    None = 0,
    Done = 1,
    Failed = 2,
}