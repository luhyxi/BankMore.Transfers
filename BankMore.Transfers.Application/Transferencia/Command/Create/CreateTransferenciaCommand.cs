using Mediator;
using SharedKernel;

namespace BankMore.Transfers.Application.Transferencia.Command.Create;

public sealed record CreateTransferenciaCommand(
    string JwtToken,
    string SenderAccountNumber,
    string ReceiverAccountNumber,
    decimal Amount)
    : ICommand<CreateTransferenciaResult>;
