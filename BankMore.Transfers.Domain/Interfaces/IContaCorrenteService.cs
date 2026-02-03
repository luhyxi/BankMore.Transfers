namespace BankMore.Transfers.Domain.Interfaces;

public interface IContaCorrenteService
{
    ValueTask<bool> RealizeDebit(string apiToken, decimal valor, string? accountNumber = null);
    ValueTask<bool> RealizeCredit(string apiToken, decimal valor, string? accountNumber = null);
}
