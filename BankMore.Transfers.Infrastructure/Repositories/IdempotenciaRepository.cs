using System.Data;
using BankMore.Transfers.Domain.IdempotenciaAggregate;
using BankMore.Transfers.Domain.Interfaces;
using BankMore.Transfers.Infrastructure.Data;
using Dapper;

namespace BankMore.Transfers.Infrastructure.Repositories;

public sealed class IdempotenciaRepository : IIdempotenciaRepository
{
    private const string SelectSql = """
        SELECT
            chave_idempotencia AS Id,
            requisicao AS Requisicao,
            resultado AS Resultado
        FROM idempotencia
        """;

    private const string InsertSql = """
        INSERT INTO idempotencia (
            chave_idempotencia,
            requisicao,
            resultado
        )
        VALUES (
            @Id,
            @Requisicao,
            @Resultado
        )
        """;

    private const string UpdateSql = """
        UPDATE idempotencia
        SET
            requisicao = @Requisicao,
            resultado = @Resultado
        WHERE chave_idempotencia = @Id
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public IdempotenciaRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async ValueTask CreateAsync(Idempotencia idempotencia, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var parameters = BuildParameters(idempotencia);
        var command = new CommandDefinition(InsertSql, parameters, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
    }

    public async ValueTask<Idempotencia> GetByIdAsync(Guid idempotenciaId, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var sql = $"{SelectSql} WHERE chave_idempotencia = @Id";
        var command = new CommandDefinition(sql, new { Id = idempotenciaId }, cancellationToken: cancellationToken);
        var result = await connection.QuerySingleOrDefaultAsync<IdempotenciaRow>(command);
        return result is null
            ? throw new KeyNotFoundException($"Idempotencia with id '{idempotenciaId}' was not found.")
            : Map(result);
    }

    public async ValueTask<Idempotencia> GetByRequisicaoAsync(string requisicao, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var sql = $"{SelectSql} WHERE requisicao = @Requisicao";
        var command = new CommandDefinition(sql, new { Requisicao = requisicao }, cancellationToken: cancellationToken);
        var result = await connection.QuerySingleOrDefaultAsync<IdempotenciaRow>(command);
        return result is null
            ? throw new KeyNotFoundException($"Idempotencia with requisicao '{requisicao}' was not found.")
            : Map(result);
    }

    public async ValueTask UpdateAsync(Idempotencia idempotencia, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var parameters = BuildParameters(idempotencia);
        var command = new CommandDefinition(UpdateSql, parameters, cancellationToken: cancellationToken);
        var affected = await connection.ExecuteAsync(command);
        if (affected == 0)
        {
            throw new KeyNotFoundException($"Idempotencia with id '{idempotencia.IdempotenciaId}' was not found.");
        }
    }

    private async ValueTask<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await _connectionFactory.OpenConnectionAsync(cancellationToken);
    }

    private static object BuildParameters(Idempotencia idempotencia)
    {
        return new
        {
            Id = idempotencia.IdempotenciaId,
            Requisicao = idempotencia.Requisicao,
            Resultado = (uint)idempotencia.Resultado
        };
    }

    private static Idempotencia Map(IdempotenciaRow row)
    {
        return Idempotencia.Load(
            Guid.Parse(row.Id),
            row.Requisicao,
            ParseResultado(row.Resultado));
    }

    private static IdempotenciaResult ParseResultado(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return IdempotenciaResult.None;
        }

        if (Enum.TryParse<IdempotenciaResult>(raw, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        if (uint.TryParse(raw, out var numeric))
        {
            return (IdempotenciaResult)numeric;
        }

        return IdempotenciaResult.None;
    }

    private sealed class IdempotenciaRow
    {
        public string Id { get; set; } = string.Empty;
        public string? Requisicao { get; set; }
        public string? Resultado { get; set; }
    }
}
