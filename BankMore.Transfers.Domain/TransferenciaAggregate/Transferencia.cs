using SharedKernel;

namespace BankMore.Transfers.Domain.TransferenciaAggregate;

public sealed class Transferencia : IAggregateRoot
{
    public string IdContaCorrenteOrigem  { get; set; } // Precisa usar o UUID da table de ContaOrigem no futuro
    public string IdContaCorrenteDestino { get; set; }
    public DateTime DataMovimento  { get; set; }
    public decimal Valor { get; set; }
}