using BankMore.Transfers.Domain.TransferenciaAggregate;

namespace BankMore.Transfers.UnitTests.Application;

public class TransferenciaTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_RejectsMissingIdTransferencia(string? idTransferencia)
    {
        var exception = Should.Throw<ArgumentException>(() =>
            new Transferencia(
                idTransferencia!,
                "origem",
                "destino",
                new DateTime(2024, 1, 2),
                10m));

        exception.ParamName.ShouldBe("idTransferencia");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_RejectsMissingIdContaCorrenteOrigem(string? idContaCorrenteOrigem)
    {
        var exception = Should.Throw<ArgumentException>(() =>
            new Transferencia(
                "transferencia",
                idContaCorrenteOrigem!,
                "destino",
                new DateTime(2024, 1, 2),
                10m));

        exception.ParamName.ShouldBe("idContaCorrenteOrigem");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_RejectsMissingIdContaCorrenteDestino(string? idContaCorrenteDestino)
    {
        var exception = Should.Throw<ArgumentException>(() =>
            new Transferencia(
                "transferencia",
                "origem",
                idContaCorrenteDestino!,
                new DateTime(2024, 1, 2),
                10m));

        exception.ParamName.ShouldBe("idContaCorrenteDestino");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Ctor_RejectsNonPositiveValor(decimal valor)
    {
        var exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            new Transferencia(
                "transferencia",
                "origem",
                "destino",
                new DateTime(2024, 1, 2),
                valor));

        exception.ParamName.ShouldBe("valor");
    }

    [Fact]
    public void Ctor_AssignsProperties()
    {
        var dataMovimento = new DateTime(2024, 1, 2, 10, 30, 0);

        var transferencia = new Transferencia(
            "transferencia",
            "origem",
            "destino",
            dataMovimento,
            125.50m);

        transferencia.IdTransferencia.ShouldBe("transferencia");
        transferencia.IdContaCorrenteOrigem.ShouldBe("origem");
        transferencia.IdContaCorrenteDestino.ShouldBe("destino");
        transferencia.DataMovimento.ShouldBe(dataMovimento);
        transferencia.Valor.ShouldBe(125.50m);
    }
}
