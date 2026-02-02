using System.Net.Http;
using BankMore.Transfers.Domain.Interfaces;
using Mediator;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace BankMore.Transfers.Application.Transferencia.Command.Create;

public sealed class CreateTransferenciaHandler(
    ITransferenciaRepository repository,
    IContaCorrenteService service,
    ILogger<CreateTransferenciaHandler> logger
    )
    : ICommandHandler<CreateTransferenciaCommand, CreateTransferenciaResult>
{
    public async ValueTask<CreateTransferenciaResult> Handle(CreateTransferenciaCommand command, CancellationToken cancellationToken)
    {
        if (IsCommandValid(command, out var result))
            return CreateTransferenciaResult.Failure(result!);

        logger.LogInformation(command.JwtToken);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(command.JwtToken);
        var senderId = jwt.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

        Guid debitTransactionId;
        try
        {
            debitTransactionId = await service.RealizeDebit(
                command.JwtToken,
                command.ReceiverAccountNumber,
                command.Amount);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Debit failed for account {AccountNumber}", command.SenderAccountNumber);
            return CreateTransferenciaResult.Failure($"debit_failed: {ex.Message}");
        }

        if (debitTransactionId == Guid.Empty)
        {
            return CreateTransferenciaResult.Failure("debit_failed: checking account service returned an empty transaction id.");
        }

        try
        {
            var creditTransactionId = await service.RealizeCredit(
                command.JwtToken,
                command.ReceiverAccountNumber,
                command.Amount);

            if (creditTransactionId == Guid.Empty)
            {
                var reversalMessage = await TryReverseAsync(command);
                return CreateTransferenciaResult.Failure(
                    $"credit_failed: checking account service returned an empty transaction id. {reversalMessage}");
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Credit failed for account {AccountNumber}", command.ReceiverAccountNumber);
            var reversalMessage = await TryReverseAsync(command);
            return CreateTransferenciaResult.Failure($"credit_failed: {ex.Message}. {reversalMessage}");
        }

        try
        {
            var transferencia = new Domain.TransferenciaAggregate.Transferencia
            {
                IdContaCorrenteOrigem = senderId!,
                IdContaCorrenteDestino = command.ReceiverAccountNumber,
                DataMovimento = DateTime.UtcNow,
                Valor = command.Amount
            };

            await repository.CreateAsync(transferencia, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to persist transfer for request");
            return CreateTransferenciaResult.Failure($"persistence_failed: {ex.Message}");
        }

        return CreateTransferenciaResult.Success(Guid.NewGuid());
    }

    private static bool IsCommandValid(CreateTransferenciaCommand command, out string? result)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.JwtToken))
        {
            result = "Authentication token must be provided.";
            return true;
        }

        if (command.Amount <= 0)
        {
            result = "Transfer amount must be greater than zero.";
            return true;
        }

        if (string.IsNullOrWhiteSpace(command.ReceiverAccountNumber))
        {
            result = "Destination account number must be provided.";
            return true;
        }

        result = null;
        return false;
    }

    private async Task<string> TryReverseAsync(CreateTransferenciaCommand command)
    {
        try
        {
            var reversalId = await service.RealizeDebit(
                command.JwtToken,
                command.ReceiverAccountNumber,
                command.Amount);

            return reversalId == Guid.Empty
                ? "reversal_failed: checking account service returned an empty transaction id."
                : "reversal_performed: debit reversal succeeded.";
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Reversal failed for account {AccountNumber}", command.SenderAccountNumber);
            return $"reversal_failed: {ex.Message}";
        }
    }
}
