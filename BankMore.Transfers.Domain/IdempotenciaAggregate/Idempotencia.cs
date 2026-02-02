using SharedKernel;

namespace BankMore.Transfers.Domain.IdempotenciaAggregate;

public sealed class Idempotencia : IAggregateRoot
{
    public Guid IdempotenciaId { get; private set; }

    public string? Requisicao { get; private set; }

    public IdempotenciaResult Resultado { get; private set; }

    public Guid ChaveIdempotencia => IdempotenciaId;

    private Idempotencia(Guid idempotenciaId, string? requisicao, IdempotenciaResult resultado)
    {
        IdempotenciaId = idempotenciaId;
        Requisicao = requisicao;
        Resultado = resultado;
    }

    private Idempotencia()
    {
    }

    public static Idempotencia Create(string? requisicao = null)
    {
        return new Idempotencia(
            Guid.NewGuid(),
            requisicao,
            IdempotenciaResult.None
        );
    }

    public static Idempotencia Load(Guid idempotenciaId, string? requisicao, IdempotenciaResult resultado)
    {
        return new Idempotencia(idempotenciaId, requisicao, resultado);
    }

    public void SetResultado(IdempotenciaResult resultado)
    {
        Resultado = resultado;
    }
}
