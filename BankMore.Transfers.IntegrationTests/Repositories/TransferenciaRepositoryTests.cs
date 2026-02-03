using BankMore.Transfers.Domain.Interfaces;
using BankMore.Transfers.Domain.TransferenciaAggregate;
using BankMore.Transfers.Infrastructure.Data;
using BankMore.Transfers.Infrastructure.Repositories;
using BankMore.Transfers.IntegrationTests.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace BankMore.Transfers.IntegrationTests.Repositories;

public class TransferenciaRepositoryTests : IDisposable
{
    private readonly ServiceProvider _provider;
    private readonly SqliteConnection _connection;

    public TransferenciaRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        TransferenciaSchema.Create(_connection);

        var services = new ServiceCollection();

        // Real factory instance
        var factory = new Data.SqliteConnectionFactory(_connection);

        services.AddSingleton<IDbConnectionFactory>(factory);
        services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();

        _provider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task Should_Create_And_Get_Transferencia()
    {
        // Arrange
        var repo = _provider.GetRequiredService<ITransferenciaRepository>();

        var transferencia = new Transferencia(
            Guid.NewGuid().ToString(),
            "conta-origem",
            "conta-destino",
            DateTime.UtcNow.Date,
            150.75m
        );

        // Act
        await repo.CreateAsync(transferencia);
        var result = await repo.GetByIdAsync(Guid.Parse(transferencia.IdTransferencia), default);

        // Assert
        result.ShouldNotBeNull();
        result.IdTransferencia.ShouldBe(transferencia.IdTransferencia);
        result.IdContaCorrenteOrigem.ShouldBe("conta-origem");
        result.IdContaCorrenteDestino.ShouldBe("conta-destino");
        result.Valor.ShouldBe(150.75m);
    }

    [Fact]
    public async Task Should_Throw_When_Transferencia_Not_Found()
    {
        var repo = _provider.GetRequiredService<ITransferenciaRepository>();

        var act = () => repo.GetByIdAsync(Guid.NewGuid()).AsTask();

        await Should.ThrowAsync<KeyNotFoundException>(act);
    }

    public void Dispose()
    {
        _connection.Dispose();
        _provider.Dispose();
    }
}
