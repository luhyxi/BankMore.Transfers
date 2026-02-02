using BankMore.Transfers.Domain.TransferenciaAggregate;
using SharedKernel;

namespace BankMore.Transfers.Domain.Interfaces;

public interface ITransferenciaRepository
{
    ValueTask CreateAsync(Transferencia transferencia, CancellationToken cancellationToken = default);
    ValueTask<Transferencia> GetByIdAsync(Guid transferenciaId, CancellationToken cancellationToken = default);
}