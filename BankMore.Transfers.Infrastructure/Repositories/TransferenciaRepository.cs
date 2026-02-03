using System.Data;
using System.Globalization;
using BankMore.Transfers.Domain.Interfaces;
using BankMore.Transfers.Domain.TransferenciaAggregate;
using BankMore.Transfers.Infrastructure.Data;
using Dapper;

namespace BankMore.Transfers.Infrastructure.Repositories;

public sealed class TransferenciaRepository : ITransferenciaRepository
{
    private const string SelectSql = """
        SELECT
            idtransferencia AS IdTransferencia,
            idcontacorrente_origem AS IdContaCorrenteOrigem,
            idcontacorrente_destino AS IdContaCorrenteDestino,
            datamovimento AS DataMovimento,
            valor AS Valor
        FROM transferencia
        """;

    private const string InsertSql = """
        INSERT INTO transferencia (
            idtransferencia,
            idcontacorrente_origem,
            idcontacorrente_destino,
            datamovimento,
            valor
        )
        VALUES (
            @IdTransferencia,
            @IdContaCorrenteOrigem,
            @IdContaCorrenteDestino,
            @DataMovimento,
            @Valor
        )
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public TransferenciaRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async ValueTask CreateAsync(Transferencia transferencia, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var parameters = BuildParameters(transferencia);
        var command = new CommandDefinition(InsertSql, parameters, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
    }

    public async ValueTask<Transferencia> GetByIdAsync(Guid transferenciaId, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var sql = $"{SelectSql} WHERE idtransferencia = @Id";
        var command = new CommandDefinition(sql, new { Id = transferenciaId.ToString() }, cancellationToken: cancellationToken);
        var result = await connection.QuerySingleOrDefaultAsync<TransferenciaRow>(command);
        return result is null
            ? throw new KeyNotFoundException($"Transferencia with id '{transferenciaId}' was not found.")
            : Map(result);
    }

    private async ValueTask<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await _connectionFactory.OpenConnectionAsync(cancellationToken);
    }

    private static object BuildParameters(Transferencia transferencia)
    {
        return new
        {
            IdTransferencia = transferencia.IdTransferencia,
            IdContaCorrenteOrigem = transferencia.IdContaCorrenteOrigem,
            IdContaCorrenteDestino = transferencia.IdContaCorrenteDestino,
            DataMovimento = transferencia.DataMovimento.ToString("dd/MM/yyyy"),
            Valor = transferencia.Valor
        };
    }

    private static Transferencia Map(TransferenciaRow row)
    {
        return new Transferencia(
            row.IdTransferencia,
            row.IdContaCorrenteOrigem,
            row.IdContaCorrenteDestino,
            DateTime.ParseExact(row.DataMovimento, "dd/MM/yyyy", CultureInfo.InvariantCulture),
            row.Valor
        );
    }

    private sealed class TransferenciaRow
    {
        public string IdTransferencia { get; set; } = string.Empty;
        public string IdContaCorrenteOrigem { get; set; } = string.Empty;
        public string IdContaCorrenteDestino { get; set; } = string.Empty;
        public string DataMovimento { get; set; } = string.Empty;
        public decimal Valor { get; set; }
    }
}