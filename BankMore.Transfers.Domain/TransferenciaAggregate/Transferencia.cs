using SharedKernel;

namespace BankMore.Transfers.Domain.TransferenciaAggregate;

public sealed class Transferencia : IAggregateRoot
{

    public Transferencia(
        string idTransferencia,
        string idContaCorrenteOrigem,
        string idContaCorrenteDestino,
        DateTime dataMovimento,
        decimal valor)
    {
        if (string.IsNullOrWhiteSpace(idTransferencia))
            throw new ArgumentException("IdTransferencia is required.", nameof(idTransferencia));

        if (string.IsNullOrWhiteSpace(idContaCorrenteOrigem))
            throw new ArgumentException("IdContaCorrenteOrigem is required.", nameof(idContaCorrenteOrigem));

        if (string.IsNullOrWhiteSpace(idContaCorrenteDestino))
            throw new ArgumentException("IdContaCorrenteDestino is required.", nameof(idContaCorrenteDestino));

        if (valor <= 0)
            throw new ArgumentOutOfRangeException(nameof(valor), valor, "Valor must be greater than zero.");

        IdTransferencia = idTransferencia;
        IdContaCorrenteOrigem = idContaCorrenteOrigem;
        IdContaCorrenteDestino = idContaCorrenteDestino;
        DataMovimento = dataMovimento;
        Valor = valor;
    }

    public string IdTransferencia { get; set; } // Precisa usar o UUID da table de ContaOrigem no futuro
    public string IdContaCorrenteOrigem { get; set; } // Precisa usar o UUID da table de ContaOrigem no futuro
    public string IdContaCorrenteDestino { get; set; }
    public DateTime DataMovimento { get; set; }
    public decimal Valor { get; set; }
}