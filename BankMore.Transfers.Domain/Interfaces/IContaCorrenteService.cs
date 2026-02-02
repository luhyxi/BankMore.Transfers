namespace BankMore.Transfers.Domain.Interfaces;

public interface IContaCorrenteService
{
    ValueTask<Guid> RealizeDebit(string apiToken, string accountNumber, decimal valor);
    ValueTask<Guid> RealizeCredit(string apiToken, string accountNumber, decimal valor);
}
