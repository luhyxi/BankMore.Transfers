using BankMore.Transfers.Domain.IdempotenciaAggregate;

namespace BankMore.Transfers.UnitTests.Application;

public class IdempotenciaTests
{
    [Fact]
    public void Create_AssignsDefaults()
    {
        var idempotencia = Idempotencia.Create("payload");

        idempotencia.IdempotenciaId.ShouldNotBe(Guid.Empty);
        idempotencia.ChaveIdempotencia.ShouldBe(idempotencia.IdempotenciaId);
        idempotencia.Requisicao.ShouldBe("payload");
        idempotencia.Resultado.ShouldBe(IdempotenciaResult.None);
    }

    [Fact]
    public void Create_AllowsNullRequest()
    {
        var idempotencia = Idempotencia.Create();

        idempotencia.Requisicao.ShouldBeNull();
        idempotencia.Resultado.ShouldBe(IdempotenciaResult.None);
    }

    [Fact]
    public void Load_AssignsProvidedValues()
    {
        var id = Guid.NewGuid();

        var idempotencia = Idempotencia.Load(id, "request", IdempotenciaResult.Done);

        idempotencia.IdempotenciaId.ShouldBe(id);
        idempotencia.ChaveIdempotencia.ShouldBe(id);
        idempotencia.Requisicao.ShouldBe("request");
        idempotencia.Resultado.ShouldBe(IdempotenciaResult.Done);
    }

    [Fact]
    public void SetResultado_UpdatesResult()
    {
        var idempotencia = Idempotencia.Create();

        idempotencia.SetResultado(IdempotenciaResult.Failed);

        idempotencia.Resultado.ShouldBe(IdempotenciaResult.Failed);
    }
}
