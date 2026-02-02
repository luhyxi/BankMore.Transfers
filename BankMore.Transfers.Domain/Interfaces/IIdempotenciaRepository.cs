using BankMore.Transfers.Domain.IdempotenciaAggregate;

namespace BankMore.Transfers.Domain.Interfaces;

public interface IIdempotenciaRepository
{
    ValueTask CreateAsync(Idempotencia idempotencia, CancellationToken cancellationToken = default);

    ValueTask<Idempotencia> GetByIdAsync(Guid idempotenciaId, CancellationToken cancellationToken = default);

    ValueTask<Idempotencia> GetByRequisicaoAsync(string requisicao, CancellationToken cancellationToken = default);

    ValueTask UpdateAsync(Idempotencia idempotencia, CancellationToken cancellationToken = default);
}
