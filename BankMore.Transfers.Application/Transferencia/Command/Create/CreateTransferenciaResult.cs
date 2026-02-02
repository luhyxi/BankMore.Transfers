using SharedKernel;

namespace BankMore.Transfers.Application.Transferencia.Command.Create;

public sealed class CreateTransferenciaResult : IResult
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public Guid? EventId { get; }

    private CreateTransferenciaResult(bool isSuccess, Guid eventId, string? error)
    {
        IsSuccess = isSuccess;
        EventId = eventId;
        Error = error;
    }

    public static CreateTransferenciaResult Success(Guid eventId)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("EventId must be a non-empty GUID.", nameof(eventId));

        return new CreateTransferenciaResult(isSuccess: true, eventId: eventId, error: null);
    }

    public static CreateTransferenciaResult Failure(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Error message must be provided.", nameof(error));

        return new CreateTransferenciaResult(isSuccess: false, eventId: Guid.Empty, error: error);
    }
}